using System.Configuration;

namespace YoungCloud.Configurations.Exceptions
{
    /// <summary>
    /// The mail receiver setting of <see cref="MailLogElement">MailLogElement</see>.
    /// </summary>
    public class ReceiverElement : ConfigurationElement
    {
        /// <summary>
        /// The mail address of receiver.
        /// </summary>
        [ConfigurationProperty("Receiver")]
        public string Receiver
        {
            get
            {
                return (string)this["Receiver"];
            }
        }
    }
}