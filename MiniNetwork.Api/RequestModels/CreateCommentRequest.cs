using System.ComponentModel.DataAnnotations;

namespace MiniNetwork.Api.RequestModels
{
    public class CreateCommentRequest
    {
        [Required]
        [MaxLength(1000)]
        public string Content { get; set; } = string.Empty;
    }
}
