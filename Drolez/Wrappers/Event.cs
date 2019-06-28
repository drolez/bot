namespace Drolez.Wrappers
{
    /// <summary>
    /// Event wrapper
    /// </summary>
    public class Event
    {
        /// <summary>
        /// Event data
        /// </summary>
        public object Data { get; set; } = "Empty";

        /// <summary>
        /// Event descriptor
        /// </summary>
        public string DrolezEventMutationDescriptor { get; set; } = "error";

        /// <summary>
        /// Convert to JSON event string
        /// </summary>
        public string ToEventJSON()
        {
            string invalid = this.ToJSON();
            return invalid.Replace("\"DrolezEventMutationDescriptor\":", "\"mutation\":");
        }
    }
}
