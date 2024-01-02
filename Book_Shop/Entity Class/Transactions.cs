namespace Book_Shop.Entity_Class
{
    public class Transactions
    {
        public int Id { get; set; }
        public int user_ID { get; set; }
        public string Method { get; set; }
        public int BooksQty { get; set; }
        public double Total { get; set; }
        public string Created_at { get; set; }
    }
}
