namespace Connect4.Models
{
    public class JugadoresModel
    {

    }

    public class JugadoresJuego
    {
        public long Id { get; set; }
        public string? Nombre { get; set; }
    }

    public class JugadoresEnPartida
    {
        public string? NombreJugador1 { get; set; }
        public string? NombreJugador2 { get; set; }
        public long IdJugador1 { get; set; }
        public long IdJugador2 { get; set; }
        public int TurnoActual { get; set; }
    }
}
