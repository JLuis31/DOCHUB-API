using System.ComponentModel.DataAnnotations;

namespace DOCHUB.APP.Models
{
    public class Usuario
    {
        [Key]
        public long id { get; set; }
        public string nombre { get; set; }
        public string email { get; set; }
        public string password { get; set; }
        public DateTime fechacreacion { get; set; }
        public string refreshtoken { get; set; }
        public DateTime refreshtokenexpirytime { get; set; }
    }


}