namespace Api.Dto
{
    public class UploadResultDto
    {
        public Guid Id { get; set; }  
        public string? FileName { get; set; }
        public string? ContentType { get; set; }
    }
}
