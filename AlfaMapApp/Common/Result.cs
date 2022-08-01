using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlfaMap.Common {
    public abstract class Result<T, E> where E : Exception {
        public abstract bool Err { get; }
        public abstract bool Ok { get; }
        public abstract E Error { get; }
        public abstract T Value { get; }

        public abstract void Map(Action<T> action);
        public abstract Result<Next, E> FlatMap<Next>(Func<T, Result<Next, E>> func);
        public abstract Task<Result<Next, E>> FlatMap<Next>(Func<T, Task<Result<Next, E>>> func);
        public abstract void MapErr<Next>(Action<E> action);
        public abstract T Unwrap();
    }


    public class ErrResult<T, E> : Result<T, E> where E : Exception {
        private E _error;
        public ErrResult(E error) {
            _error = error;
        }
        public override bool Err => true;
        public override bool Ok => false;
        public override E Error => _error;

        public override T Value => default;

        public override Result<Next, E> FlatMap<Next>(Func<T, Result<Next, E>> func) {
            return new ErrResult<Next, E>(Error);
        }

        public override Task<Result<Next, E>> FlatMap<Next>(Func<T, Task<Result<Next, E>>> func) {
            var task = new Task<Result<Next, E>>(() => new ErrResult<Next, E>(Error));
            return task;
        }

        public override void Map(Action<T> action) {
            return;
        }

        public override void MapErr<Next>(Action<E> action) {
            action(Error);
        }

        public override T Unwrap() {
            throw Error;
        }
    }

    public class OkResult<T, E> : Result<T, E> where E : Exception {
        private readonly T _data;
        public OkResult(T data) {
            _data = data;
        }
        public override bool Err => false;
        public override bool Ok => true;
        public override E Error => default;
        public override T Value => _data;

        public override Result<Next, E> FlatMap<Next>(Func<T, Result<Next, E>> func) {
            return func(Value);
        }

        public override Task<Result<Next, E>> FlatMap<Next>(Func<T, Task<Result<Next, E>>> func) {
            return func(Value);
        }

        public override void Map(Action<T> action) {
            action(Value);
        }

        public override void MapErr<Next>(Action<E> action) {
            return;
        }

        public override T Unwrap() {
            return Value;
        }
    }
}