namespace ControleDePatio.Models
{
    public class Vaga
    {
        public int Id { get; set; }

        public string Identificacao { get; set; } = string.Empty;

        public bool EstaOcupada { get; set; } = false;
    }
}
