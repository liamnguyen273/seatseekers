namespace com.brg.UnityComponents
{
    public interface IMenuOption
    {
        public string Category { get; set; }
        public int DesiredOrder { get; set;}
        public string Id { get; set;}

        public string GetIdInMenu() => $"{Category}_{Id}";
    }
}