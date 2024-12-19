using System.ComponentModel.DataAnnotations;

namespace Api.Model
{
    public class Modeling
    {
        [Key]
        public Guid Id { get; set; }

        public AppUser Owner { get; set; }

        public string Name { get; set; }    

        public string Description { get; set; } 

        public string Category  { get; set; }

        public decimal Price { get; set; }

        public string[]? Models {  get; set; }
        
        public DateTime? CreatedDate { get; set; }= DateTime.UtcNow;

        public DateTime? UpdatedDate { get;set; }= DateTime.UtcNow; 


    }
}
