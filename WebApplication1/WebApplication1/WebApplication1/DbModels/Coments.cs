﻿namespace WebApplication1.DbModels
{
    public class Coments
    {
        public int Id { get; set; }
        public string Text{ get; set; }
        public int PsotId { get; set; }
        public int AuthorId { get; set; }
    }
}
