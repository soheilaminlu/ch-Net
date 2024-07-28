namespace WebApplication1.Models
{
    public class UserModel
    {
        public int? Id { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }  
        
        public int? Age { get; set; }

        public string? Website { get; set; }

        //one to many 
        public List<MessageModel>? Messages { get; set; }
    }
}
