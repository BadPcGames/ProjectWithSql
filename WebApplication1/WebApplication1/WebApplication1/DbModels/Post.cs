namespace WebApplication1.DbModels
{
    public class Post
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public DateTime CreateAt{ get; set; }
        public int BlogId { get; set; }
    }
}
