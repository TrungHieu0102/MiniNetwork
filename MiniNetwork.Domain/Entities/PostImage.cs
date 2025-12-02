    using MiniNetwork.Domain.Common;

    namespace MiniNetwork.Domain.Entities;

    public class PostImage : Entity
    {
        public string Url { get; private set; } = null!;
        public Guid PostId { get; private set; }

        private PostImage() { }

        public PostImage(string url)
        {
            Url = url;
        }
    }
