using System;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Lifetime;
using System.Security;
using System.Security.Permissions;

namespace Ratatoskr.Loader
{
    /// <summary>
    /// Wraps an instance of TInterface. If the instance is a 
    /// MarshalByRefObject, this class acts as a sponsor for its lifetime 
    /// service (until disposed/finalized). Disposing the sponsor implicitly 
    /// disposes the instance.
    /// </summary>
    /// <typeparam name="TInterface"></typeparam>
    [Serializable]
    [SecurityPermission(SecurityAction.Demand, Infrastructure = true)]
    public sealed class Sponsor<TInterface> : ISponsor, IDisposable
        where TInterface : class
    {
        private TInterface instance;

        /// <summary>
        /// Initializes a new instance of the <see cref="Sponsor{TInterface}"/> class,
        /// wrapping the specified object instance.
        /// </summary>
        /// <param name="instance">
        /// </param>
        /// <exception cref="SecurityException">The immediate caller does not have infrastructure permission. </exception>
        public Sponsor(TInterface instance)
        {
            Instance = instance;
            if (Instance is MarshalByRefObject)
            {
                var lifetimeService =
                    RemotingServices.GetLifetimeService((MarshalByRefObject)(object)Instance) as ILease;
                lifetimeService?.Register(this);
            }
        }

        /// <summary>
        /// Finaliser.
        /// </summary>
        ~Sponsor()
        {
            Dispose(false);
        }

        /// <summary>
        /// Gets the wrapped instance of TInterface.
        /// </summary>
        /// <exception cref="ObjectDisposedException" accessor="get">If this sponsor instance has been disposed.</exception>
        public TInterface Instance
        {
            get
            {
                if (IsDisposed)
                {
                    throw new ObjectDisposedException(nameof(Instance));
                }

                return instance;
            }

            private set
            {
                instance = value;
            }
        }

        /// <summary>
        /// Gets whether the sponsor has been disposed.
        /// </summary>
        public bool IsDisposed { get; private set; }

        /// <summary>
        /// Disposes the sponsor and the instance it wraps.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Renews the lease on the instance as though it has been called normally.
        /// </summary>
        /// <param name="lease"></param>
        /// <returns></returns>
        TimeSpan ISponsor.Renewal(ILease lease)
        {
            return IsDisposed ? TimeSpan.Zero : LifetimeServices.RenewOnCallTime;
        }

        /// <summary>
        /// Disposes the sponsor and the instance it wraps.
        /// </summary>
        /// <param name="disposing"></param>
        private void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (disposing)
                {
                    if (Instance is IDisposable)
                    {
                        ((IDisposable)Instance).Dispose();
                    }

                    if (Instance is MarshalByRefObject)
                    {
                        var lifetimeService = RemotingServices.GetLifetimeService((MarshalByRefObject)(object)Instance) as ILease;
                        lifetimeService?.Unregister(this);
                    }
                }

                Instance = null;
                IsDisposed = true;
            }
        }
    }
}
