namespace ControleDePatio.Models
{
    public class Vaga
    {
        public int Id { get; set; }

        // Para podermos chamar as vagas de "A1", "A2", "Vaga 01", etc.
        public string Identificacao { get; set; } = string.Empty;

        public bool EstaOcupada { get; set; } = false;
    }
}
