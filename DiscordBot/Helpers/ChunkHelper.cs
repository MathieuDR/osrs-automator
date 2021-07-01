using System;
using System.Collections.Generic;

namespace DiscordBot.Helpers {
    internal class ChunkedEnumerable<T> : IEnumerable<T> {
        private readonly int _chunkSize;
        private readonly int _start;

        private readonly EnumeratorWrapper<T> _wrapper;

        public ChunkedEnumerable(EnumeratorWrapper<T> wrapper, int chunkSize, int start) {
            _wrapper = wrapper;
            _chunkSize = chunkSize;
            _start = start;
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        public IEnumerator<T> GetEnumerator() {
            return new ChildEnumerator(this);
        }

        private class ChildEnumerator : IEnumerator<T> {
            private readonly ChunkedEnumerable<T> _parent;
            private T _current;
            private bool _done;
            private int _position;


            public ChildEnumerator(ChunkedEnumerable<T> parent) {
                _parent = parent;
                _position = -1;
                parent._wrapper.AddRef();
            }

            public void Dispose() {
                if (!_done) {
                    _done = true;
                    _parent._wrapper.RemoveRef();
                }
            }

            object System.Collections.IEnumerator.Current {
                get { return Current; }
            }

            public bool MoveNext() {
                _position++;

                if (_position + 1 > _parent._chunkSize) {
                    _done = true;
                }

                if (!_done) {
                    _done = !_parent._wrapper.Get(_position + _parent._start, out _current);
                }

                return !_done;
            }

            public void Reset() {
                // per http://msdn.microsoft.com/en-us/library/system.collections.ienumerator.reset.aspx
                throw new NotSupportedException();
            }

            public T Current {
                get {
                    if (_position == -1 || _done) {
                        throw new InvalidOperationException();
                    }

                    return _current;
                }
            }
        }
    }

    internal class EnumeratorWrapper<T> {
        private Enumeration _currentEnumeration;

        private int _refs;

        public EnumeratorWrapper(IEnumerable<T> source) {
            SourceEumerable = source;
        }

        private IEnumerable<T> SourceEumerable { get; set; }

        public bool Get(int pos, out T item) {
            if (_currentEnumeration != null && _currentEnumeration.Position > pos) {
                _currentEnumeration.Source.Dispose();
                _currentEnumeration = null;
            }

            if (_currentEnumeration == null) {
                _currentEnumeration = new Enumeration {Position = -1, Source = SourceEumerable.GetEnumerator(), AtEnd = false};
            }

            item = default(T);
            if (_currentEnumeration.AtEnd) {
                return false;
            }

            while (_currentEnumeration.Position < pos) {
                _currentEnumeration.AtEnd = !_currentEnumeration.Source.MoveNext();
                _currentEnumeration.Position++;

                if (_currentEnumeration.AtEnd) {
                    return false;
                }
            }

            item = _currentEnumeration.Source.Current;

            return true;
        }

        // needed for dispose semantics 
        public void AddRef() {
            _refs++;
        }

        public void RemoveRef() {
            _refs--;
            if (_refs == 0 && _currentEnumeration != null) {
                var copy = _currentEnumeration;
                _currentEnumeration = null;
                copy.Source.Dispose();
            }
        }

        private class Enumeration {
            public IEnumerator<T> Source { get; set; }
            public int Position { get; set; }
            public bool AtEnd { get; set; }
        }
    }
}