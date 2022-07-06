using System.ComponentModel.DataAnnotations;

namespace MessagePipeSamples
{
    public class Request
    {
        [Required]
        public byte type { get; set; }
    }
}
