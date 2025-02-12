namespace Shopping_Tutorial.Models
{
    public class Paginate
    {
        public int TotalItems { get; set; }

        public int PageSize { get; set; }

        public int CurrentPage { get; set; }

        public int TotalPages { get; set; }

        public int StartPage { get; set; }

        public int EndPage { get; set; }

        public Paginate()
        {
            
        }

        public Paginate(int totalItems, int page, int pageSize = 10)
        {
            //Ground sum of items/10 items in 1 page. Ex: 16 items/10 => ground 2 page
            int totalPages = (int)Math.Ceiling((decimal)totalItems / (decimal)pageSize);

            int currentPage = page; //current page is 1

            int startPage = CurrentPage - 5;
            int endPage = EndPage + 4;

            if(startPage <= 0)
            {
                endPage -= (startPage - 1);
                startPage = 1;
            }

            if(endPage > totalPages)
            {
                endPage = totalPages;
                if(endPage > 10)
                    startPage = endPage - 9;
                
            }
            TotalItems = totalItems;
            CurrentPage = currentPage;
            PageSize = pageSize;
            TotalPages = totalPages;
            StartPage = startPage;
            EndPage = endPage;
        }
    }
}
