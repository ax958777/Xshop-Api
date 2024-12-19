using Api.Model;

namespace Api.Dto
{
    public class ModelingRequestDto
    {

        public string Name { get; set; }

        public string Description { get; set; }

        public string Category { get; set; }

        public decimal Price { get; set; }

        public string[]? Models { get; set; }

    }
}
