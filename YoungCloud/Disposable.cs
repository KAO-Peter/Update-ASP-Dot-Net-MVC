using System;

namespace YoungCloud
{
    /// <summary>
    /// The base object of disposable classes.
    /// </summary>
    public abstract class Disposable : IDisposable
    {
        /// <summary>
        /// Destructor.
        /// </summary>
        ~Disposable()
        {
            Dispose(false);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Disposable">Disposable</see> class.
        /// </summary>
        protected Disposable()
        {
            Disposed = false;
        }

        /// <summary>
        /// Release all resources.
        /// </summary>
        public void Close()
        {
            Dispose();
        }

        /// <summary>
        /// Release all resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Release all resources.
        /// </summary>
        /// <param name="disposing">Is invoked from Dispose method or not.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!Disposed)
            {               
                Disposed = true;
            }
        }

        /// <summary>
        /// Resouces has been disposed or not.
        /// </summary>
        protected bool Disposed
        {
            get;
            private set;
        }
    }
}