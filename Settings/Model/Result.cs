namespace ChatService.Settings.Model
{
    public class Result
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public object Data { get; set; }
        public string ExcMessage { get; set; }
    }
}
