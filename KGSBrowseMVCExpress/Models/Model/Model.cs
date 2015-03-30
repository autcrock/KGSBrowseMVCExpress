namespace Well.Models.Model
{
    public class Model
    {
        public string ReturnedValue;

        public Model()
        {
            ReturnedValue = "Empty Model";
        }
        public Model(string message)
        {
            ReturnedValue = message;
        }
    }
}