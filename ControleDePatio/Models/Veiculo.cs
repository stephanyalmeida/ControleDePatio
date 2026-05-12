namespace ControleDePatio.Models
{
    public class Veiculo
    {
        // O Id será de 1 a 10, representando tanto o trator quanto a vaga fixa dele
        public int Id { get; set; }

        public string Modelo { get; set; } = string.Empty;

        public string Placa { get; set; } = string.Empty;

        // Todo veículo começa estacionado no pátio por padrão
        public bool EstaNoPatio { get; set; } = true;
        public int? VagaId { get; set; }
    }
}
