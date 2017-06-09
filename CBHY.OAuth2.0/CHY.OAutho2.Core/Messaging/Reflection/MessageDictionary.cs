using CHY.OAuth2.Core.Messaging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CHY.OAuth2.Core.Messaging.Reflection
{
    public class MessageDictionary : IDictionary<string, string>
    {
        private readonly IMessage message;
        private readonly MessageDescription description;
        private readonly bool getOriginalValues;

        public MessageDictionary(IMessage message, MessageDescription description, bool getOriginalValues)
        {
            this.message = message;
            this.description = description;
            this.getOriginalValues = getOriginalValues;
        }

        public IMessage Message
        {
            get { return this.message; }
        }

        public MessageDescription Description
        {
            get { return this.description; }
        }

        public int Count
        {
            get { return this.Keys.Count; }
        }

        bool ICollection<KeyValuePair<string, string>>.IsReadOnly
        {
            get { return false; }
        }

        public ICollection<string> Keys
        {
            get
            {
                List<string> keys = new List<string>(this.message.ExtraData.Count + this.description.Mapping.Count);
                keys.AddRange(this.DeclareKeys);
                keys.AddRange(this.AdditionalKeys);
                return keys.AsReadOnly();
            }
        }

        public ICollection<string> DeclareKeys
        {
            get
            {
                List<string> keys = new List<string>(this.description.Mapping.Count);
                foreach(var pair in this.description.Mapping)
                {
                    if(pair.Value.GetValue(this.message, this.getOriginalValues) != null)
                    {
                        keys.Add(pair.Key);
                    }
                }

                return keys.AsReadOnly();
            }
        }

        public ICollection<string> AdditionalKeys
        {
            get { return this.message.ExtraData.Keys; }
        }

        public ICollection<string> Values
        {
            get
            {
                List<string> values = new List<string>(this.message.ExtraData.Count + this.description.Mapping.Count);
                foreach(MessagePart part in this.description.Mapping.Values)
                {
                    if(part.GetValue(this.message, this.getOriginalValues) != null)
                    {
                        values.Add(part.GetValue(this.message, this.getOriginalValues));
                    }
                }

                foreach(string value in this.message.ExtraData.Values)
                {
                    Debug.Assert(value != null, "Null values should never be allowed in the extra data dictionary");
                    values.Add(value);
                }

                return values.AsReadOnly();
            }
        }

        private MessageSerializer Serializer
        {
            get { return MessageSerializer.Get(this.message.GetType()); }
        }

        public string this[string key]
        {
            get
            {
                MessagePart part;
                if (this.description.Mapping.TryGetValue(key, out part))
                {
                    return part.GetValue(this.message, this.getOriginalValues);
                }
                else
                {
                    return this.message.ExtraData[key];
                }
            }

            set
            {
                MessagePart part;
                if(this.description.Mapping.TryGetValue(key, out part))
                {
                    part.SetValue(this.message, value);
                }
                else
                {
                    if(value == null)
                    {
                        this.message.ExtraData.Remove(key);
                    }
                    else
                    {
                        this.message.ExtraData[key] = value;
                    }
                }
            }
        }

        public void Add(string key, string value)
        {
            ErrorUtilities.VerifyArgumentNotNull(value, "value");

            MessagePart part;
            if(this.description.Mapping.TryGetValue(key, out part))
            {
                if(part.IsNondefaultValueSet(this.message))
                {
                    throw new ArgumentException(MessagingStrings.KeyAlreadyExists);
                }
                part.SetValue(this.message, value);
            }
            else
            {
                this.message.ExtraData.Add(key, value);
            }
        }

        public bool ContainsKey(string key)
        {
            return this.message.ExtraData.ContainsKey(key) ||
                (this.description.Mapping.ContainsKey(key) && this.description.Mapping[key].GetValue(this.message, this.getOriginalValues) != null);
        }

        public bool Remove(string key)
        {
            if(this.message.ExtraData.Remove(key))
            {
                return true;
            }
            else
            {
                MessagePart part;
                if(this.description.Mapping.TryGetValue(key, out part))
                {
                    if(part.GetValue(this.message, this.getOriginalValues) != null)
                    {
                        part.SetValue(this.message, null);
                        return true;
                    }
                }

                return false;
            }
        }

        public bool TryGetValue(string key, out string value)
        {
            MessagePart part;
            if(this.description.Mapping.TryGetValue(key, out part))
            {
                value = part.GetValue(this.message, this.getOriginalValues);
                return value != null;
            }

            return this.message.ExtraData.TryGetValue(key, out value);
        }

        public void Add(KeyValuePair<string,string> item)
        {
            this.Add(item.Key, item.Value);
        }

        public void ClearValues()
        {
            foreach(string key in this.Keys)
            {
                this.Remove(key);
            }
        }

        public void Clear()
        {
            throw new NotSupportedException();
        }

        public bool Contains(KeyValuePair<string,string> item)
        {
            MessagePart part;
            if(this.description.Mapping.TryGetValue(item.Key, out part))
            {
                return string.Equals(part.GetValue(this.message, this.getOriginalValues), item.Value, StringComparison.Ordinal);
            }
            else
            {
                return this.message.ExtraData.Contains(item);
            }
        }

        void ICollection<KeyValuePair<string,string>>.CopyTo(KeyValuePair<string,string>[] array, int arrayIndex)
        {
            foreach(var pair in (IDictionary<string,string>)this)
            {
                array[arrayIndex++] = pair;
            }
        }

        public bool Remove(KeyValuePair<string,string> item)
        {
            if (((ICollection<KeyValuePair<string, string>>)this).Contains(item))
            {
                ((IDictionary<string, string>)this).Remove(item.Key);
                return true;
            }
            return false;
        }

        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            foreach (string key in this.Keys)
            {
                yield return new KeyValuePair<string, string>(key, this[key]);
            }
        }

        IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<KeyValuePair<string, string>>)this).GetEnumerator();
        }

        public IDictionary<string, string> Serialize()
        {
            return this.Serializer.Serialize(this);
        }

        public void Deserialize(IDictionary<string,string> fields)
        {
            this.Serializer.Deserialize(fields, this);
        }
    }
}
