using System.Configuration;

namespace YoungCloud.Configurations.Runtime.Remoting
{
    /// <summary>
    /// The collection of remoting service setting.
    /// </summary>
    public class RemotingServiceCollection : ConfigurationElementCollection
    {
        /// <summary>
        /// To get RemotingServiceElement instance by index.
        /// </summary>
        /// <param name="index">The index of instance.</param>
        /// <returns>The instance of RemotingServiceElement</returns>
        public RemotingServiceElement this[int index]
        {
            get
            {
                return (RemotingServiceElement)base.BaseGet(index);
            }
        }

        ///// <summary>
        ///// To get RemotingServiceElement instance by key.
        ///// </summary>
        ///// <param name="key">The key of instance.</param>
        ///// <returns>The instance of RemotingServiceElement</returns>
        //public RemotingServiceElement this[object key]
        //{
        //    get
        //    {
        //        return (RemotingServiceElement)base.BaseGet(key);
        //    }
        //}

        /// <summary>
        /// To create new element.
        /// </summary>
        /// <returns>The instance of new element.</returns>
        protected override ConfigurationElement CreateNewElement()
        {
            return new RemotingServiceElement();
        }

        /// <summary>
        /// To get RemotingServiceElement instance by key.
        /// </summary>
        /// <param name="key">The key of instance.</param>
        /// <returns>The instance of RemotingServiceElement</returns>
        public RemotingServiceElement Get(object key)
        {
            return (RemotingServiceElement)base.BaseGet(key);
        }

        /// <summary>
        /// To get the key of element.
        /// </summary>
        /// <param name="element">The instance of element.</param>
        /// <returns>The key of element.</returns>
        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((RemotingServiceElement)element).Name;
        }
    }
}