namespace WebApplication1.Dto
{
    public class MessagesDto
    {

        public int? Id { get; set; }

        public string? Content { get; set; } = string.Empty;

        public DateTime? DateCreated { get; set; } = DateTime.UtcNow;

        public DateTime? DateModified { get; set; } = DateTime.UtcNow;

        public int? views { get; set; } = 0;

        public bool? published { get; set; } = false;

        public int? UserId { get; set; }

    }
}
