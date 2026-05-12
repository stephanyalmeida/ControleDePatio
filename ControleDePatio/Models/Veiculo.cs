namespace ControleDePatio.Models
{
    public class Veiculo
    {
        public int Id { get; set; }

        public string Modelo { get; set; } = string.Empty;

        public string Placa { get; set; } = string.Empty;

        public bool EstaNoPatio { get; set; } = true;
        public int? VagaId { get; set; }
    }
}
