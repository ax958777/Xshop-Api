namespace Api.Dto
{
    public class ResetPasswordRequestDto
    {
        public string Password { get; set; }    

        public string Email { get; set; }

        public string Token { get; set; }    

    }
}
