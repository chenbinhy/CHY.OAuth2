using CHY.OAuth2.Core.Configuration;
using CHY.OAuth2.Core.Messaging;
using CHY.OAuth2.Core.Messaging.Reflection;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;

namespace CHY.OAuth2.Core
{
    public static class Util
    {
        public const string DefaultNamespace = "CHY.OAuth2";

        private static readonly Lazy<string> libraryVersionLazy = new Lazy<string>(delegate
        {
            var assembly = Assembly.GetExecutingAssembly();
            string assemblyFullName = assembly.FullName;
            bool official = assemblyFullName.Contains("PublickKeyToken=d0acff3d13b42a9d");
            assemblyFullName = assemblyFullName.Replace(assembly.GetName().Version.ToString(), AssemblyFileVersion);

            return string.Format(CultureInfo.InvariantCulture, "{0} ({1})", assemblyFullName, official?"official":"private");
        });

        private static readonly Lazy<ProductInfoHeaderValue> libraryVersionHeaderLazy = new Lazy<ProductInfoHeaderValue>(delegate {
            var assemblyName = Assembly.GetExecutingAssembly().GetName();

            return new ProductInfoHeaderValue(assemblyName.Name, AssemblyFileVersion);
        });

        private static IEmbeddedResourceRetrieval embeddedResourceRetrieval = MessagingElement.Configuration.EmbeddedResourceRetrievalProvider.CreateInstance(null, false, null);

        public static string LibraryVersion
        {
            get { return libraryVersionLazy.Value; }
        }

        public static ProductInfoHeaderValue LibraryVersionHeader
        {
            get { return libraryVersionHeaderLazy.Value; }
        }

        public static string AssemblyFileVersion
        {
            get
            {
                var assembly = Assembly.GetExecutingAssembly();
                var attributes = assembly.GetCustomAttributes(typeof(AssemblyFileVersionAttribute), false);
                if(attributes.Length == 1)
                {
                    var fileVersionAttribute = (AssemblyFileVersionAttribute)attributes[0];

                    return fileVersionAttribute.Version;
                }

                return assembly.GetName().Version.ToString();
            }
        }

        public static bool EqualsNullSafe<T>(this T first, T second) where T : class
        {
            if(object.ReferenceEquals(first, null) ^ object.ReferenceEquals(second, null))
            {
                return false;
            }

            if(object.ReferenceEquals(first, null))
            {
                return true;
            }

            return first.Equals(second);
        }

        public static object ToStringDeferred<K, V>(this IEnumerable<KeyValuePair<K, V>> pairs)
        {
            return new DelayedToString<IEnumerable<KeyValuePair<K, V>>>(
                pairs,
                p =>
                {
                    var dictionary = pairs as IDictionary<K, V>;
                    var messageDictionary = pairs as MessageDictionary;
                    StringBuilder sb = new StringBuilder(dictionary != null ? dictionary.Count * 40 : 200);
                    foreach(var pair in pairs)
                    {
                        var key = pair.Key.ToString();
                        string value = pair.Value.ToString();
                        if (messageDictionary != null && messageDictionary.Description.Mapping.ContainsKey(key) && messageDictionary.Description.Mapping[key].IsSecuritySensitive)
                        {
                            value = "********";
                        }
                        sb.AppendFormat("\t{0}: {1}{2}", key, value, Environment.NewLine);
                    }

                    return sb.ToString();
                }
                );
        }

        public static object ToStringDeferred<T>(this IEnumerable<T> list)
        {
            return ToStringDeferred<T>(list, false);
        }

        public static object ToStringDeferred<T>(this IEnumerable<T> list, bool multiLineElements)
        {
            return new DelayedToString<IEnumerable<T>>(
                list,
                l =>
                {
                    ErrorUtilities.VerifyArgumentNotNull(l, "l");
                    string newLine = Environment.NewLine;
                    StringBuilder sb = new StringBuilder();
                    if (multiLineElements)
                    {
                        sb.AppendLine("[{");
                        foreach(T obj in l)
                        {
                            string objString = obj != null ? obj.ToString() : "<NULL>";
                            objString = objString.Replace(newLine, Environment.NewLine + "\t");
                            sb.Append("\t");
                            sb.Append(objString);
                            if(!objString.EndsWith(Environment.NewLine, StringComparison.Ordinal))
                            {
                                sb.AppendLine();
                            }
                            sb.AppendLine("}, {");
                        }
                        if (sb.Length > 2 + Environment.NewLine.Length)
                        {
                            sb.Length -= 2 + Environment.NewLine.Length;
                        }
                        else
                        {
                            sb.Length -= 1 + Environment.NewLine.Length;
                        }
                        sb.Append("]");
                        return sb.ToString();
                    }
                    else
                    {
                        sb.Append("{");
                        foreach(T obj in l)
                        {
                            sb.Append(obj != null ? obj.ToString() : "<NULL>");
                            sb.AppendLine(",");
                        }
                        if(sb.Length > 1)
                        {
                            sb.Length -= 1;
                        }
                        sb.Append("}");
                        return sb.ToString();
                    }
                }
                );
        }

        public static string GetWebResourceUrl(Type someTypeInResourceAssembly, string manifestResourceName)
        {
            Page page;
            IEmbeddedResourceRetrieval retrieval;

            if (embeddedResourceRetrieval != null)
            {
                Uri url = embeddedResourceRetrieval.GetWebResourceUrl(someTypeInResourceAssembly, manifestResourceName);
                return url != null ? url.AbsoluteUri : null;
            }
            else if((page = HttpContext.Current.CurrentHandler as Page) != null){
                return page.ClientScript.GetWebResourceUrl(someTypeInResourceAssembly, manifestResourceName);
            }
            else if ((retrieval = HttpContext.Current.CurrentHandler as IEmbeddedResourceRetrieval) != null)
            {
                return retrieval.GetWebResourceUrl(someTypeInResourceAssembly, manifestResourceName).AbsoluteUri;
            }
            else
            {
                throw new InvalidOperationException(
                    string.Format(
                    CultureInfo.CurrentCulture,
                    Strings.EmbeddedResourceUrlProviderRequired,
                    string.Join(", ", new string[] { typeof(Page).FullName, typeof(IEmbeddedResourceRetrieval).FullName})
                    )
                    );
            }
        }

        public static async Task<Dictionary<TSource, TResult>> ToDictionaryAsync<TSource, TResult>(
            this IEnumerable<TSource> source, Func<TSource, Task<TResult>> transform
            )
        {
            var taskResults = source.ToDictionary(s => s, transform);
            await Task.WhenAll(taskResults.Values);
            return taskResults.ToDictionary(p => p.Key, p => p.Value.Result);
        }

        private class DelayedToString<T>
        {
            private readonly T obj;
            private readonly Func<T, string> toString;
            public DelayedToString(T obj, Func<T, string> toString)
            {
                this.obj = obj;
                this.toString = toString;
            }

            public override string ToString()
            {
                return this.toString(this.obj) ?? string.Empty;
            }
        }
    }
}
