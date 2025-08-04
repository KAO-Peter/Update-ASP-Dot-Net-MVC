using System.Configuration;

namespace YoungCloud.Configurations.Exceptions
{
    /// <summary>
    /// The address collection of mail to list.
    /// </summary>
    public class ReceiverCollection : ConfigurationElementCollection
    {
        /// <summary>
        /// To get MailToElement instance by index.
        /// </summary>
        /// <param name="index">The index of instance.</param>
        /// <returns>The instance of MailLogElement</returns>
        public ReceiverElement this[int index]
        {
            get
            {
                return (ReceiverElement)base.BaseGet(index);
            }
        }

        /// <summary>
        /// To create new element.
        /// </summary>
        /// <returns>The instance of new element.</returns>
        protected override ConfigurationElement CreateNewElement()
        {
            return new ReceiverElement();
        }

        /// <summary>
        /// To get the key of element.
        /// </summary>
        /// <param name="element">The instance of element.</param>
        /// <returns>The key of element.</returns>
        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((ReceiverElement)element).Receiver;
        }
    }
}