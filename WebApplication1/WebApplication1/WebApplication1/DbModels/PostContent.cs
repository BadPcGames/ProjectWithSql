namespace WebApplication1.DbModels
{
    public class PostContent
    {
        public int Id { get; set; }
        public int PostId { get; set; }
        public string Contenttype{ get; set; }
        public string Content { get; set; }
        public int Position {  get; set; }
    }
}
