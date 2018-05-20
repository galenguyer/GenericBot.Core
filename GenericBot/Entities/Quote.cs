namespace GenericBot.Entities
{
    public class Quote
    {
        public string Content { get; set; }
        public int Id { get; set; }
        public bool Active = true;

        public Quote()
        {
            Active = true;
        }

        public Quote(string c, int i)
        {
            Content = c;
            Id = i;
            Active = true;
        }

        public override string ToString()
        {
            return $"\"{Content}\" (#{Id})";
        }
    }
}
