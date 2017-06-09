using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CHY.OAuth2.Core.Messaging
{
    public static class EnumerableCacheExtension
    {
        public static IEnumerable<T> CacheGeneratedResults<T>(this IEnumerable<T> sequence)
        {
            if(sequence is IList<T> ||
                sequence is ICollection<T> ||
                sequence is Array ||
                sequence is EnumerableCache<T>)
            {
                return sequence;
            }

            return new EnumerableCache<T>(sequence);
        }

        private class EnumerableCache<T>:IEnumerable<T>
        {
            private List<T> cache;
            private IEnumerable<T> generator;
            private IEnumerator<T> generatorEnumerator;
            private object generatorLock = new object();
            public EnumerableCache(IEnumerable<T> generator)
            {
                this.generator = generator;
            }

            public IEnumerator<T> GetEnumerator()
            {
                if(this.generatorEnumerator == null)
                {
                    this.cache = new List<T>();
                    this.generatorEnumerator = this.generator.GetEnumerator();
                }

                return new EnumeratorCache(this);
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return this.GetEnumerator();
            }

            private class EnumeratorCache : IEnumerator<T>
            {
                private EnumerableCache<T> parent;
                private int cachePosition = -1;

                public EnumeratorCache(EnumerableCache<T> parent)
                {
                    this.parent = parent;
                }

                public T Current
                {
                    get
                    {
                        if (this.cachePosition < 0 || this.cachePosition >= this.parent.cache.Count)
                        {
                            throw new InvalidOperationException();
                        }

                        return this.parent.cache[this.cachePosition];
                    }
                }

                object System.Collections.IEnumerator.Current
                {
                    get { return this.Current; }
                }

                public void Dispose()
                {
                    this.Dispose(true);
                    GC.SuppressFinalize(this);
                }

                public bool MoveNext()
                {
                    this.cachePosition++;
                    if(this.cachePosition >= this.parent.cache.Count)
                    {
                        lock(this.parent.generatorLock)
                        {
                            if (this.parent.generatorEnumerator.MoveNext())
                            {
                                this.parent.cache.Add(this.parent.generatorEnumerator.Current);
                            }
                            else
                            {
                                return false;
                            }
                        }
                    }
                    return true;
                }

                public void Reset()
                {
                    this.cachePosition = -1;
                }

                protected virtual void Dispose(bool disposing)
                {

                }
            }
        }
    }
}
