using System.ComponentModel.DataAnnotations;

namespace MessagePipeSamples
{
    public class MyEvent
    {
        [Required]
        public int type { get; set; }
    }
}
