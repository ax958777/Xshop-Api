using System.ComponentModel.DataAnnotations;

namespace Api.Model
{
    public class AppFile
    {
        [Key]
        public Guid Id { get; set; }
        public string? FileName { get; set; }
        public string? ContentType { get; set; }
        public byte[] Content { get; set; }
    }
}
