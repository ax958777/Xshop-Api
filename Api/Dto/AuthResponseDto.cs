namespace Api.Dto
{
    public class AuthResponseDto
    {
        public string? Message { get; set; }

        public string? Token { get; set; }

        public bool IsSuccess { get; set; }
    }
}
