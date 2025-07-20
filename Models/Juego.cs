public class Juego
{
    public int IdPartida { get; set; }
    public int Filas { get; } = 6;
    public int Columnas { get; } = 7;
    public int[,] Tablero { get; set; }
    public int JugadorActual { get; set; } = 1;
    public bool GameOver { get; set; } = false;
    public int Winner { get; set; } = 0;
    public string? NombreJugador1 { get; set; }
    public string? NombreJugador2 { get; set; }

    public Juego()
    {
        Tablero = new int[Filas, Columnas];
    }

    public int InsertarFicha(int columna)
    {
        for (int fila = Filas - 1; fila >= 0; fila--)
        {
            if (Tablero[fila, columna] == 0)
            {
                Tablero[fila, columna] = JugadorActual;
                if (ComprobarGanador(fila, columna))
                {
                    GameOver = true;
                    Winner = JugadorActual;
                }
                else if (Tablerolleno())
                {
                    GameOver = true;
                    Winner = 0;
                }
                else
                {
                    JugadorActual = JugadorActual == 1 ? 2 : 1;
                }
                return fila;
            }
        }
        return -1;
    }


    private bool ComprobarGanador(int fila, int columna)
    {
        int jugador = Tablero[fila, columna];
        if (jugador == 0) return false;
        int[][] direcciones = new int[][] {
            new int[] { 0, 1 },
            new int[] { 1, 0 },
            new int[] { 1, 1 },
            new int[] { 1, -1 }
        };
        foreach (var direccion in direcciones)
        {
            int i = 1;
            i = i + Contador(fila, columna, direccion[0], direccion[1], jugador);
            i = i + Contador(fila, columna, -direccion[0], -direccion[1], jugador);
            if (i >= 4)
            {
                return true;
            }
        }
        return false;
    }

    private int Contador(int fila, int columna, int cfila, int ccolumna, int jugador)
    {
        int contador = 0;
        int f = fila + cfila;
        int c = columna + ccolumna;

        while (f >= 0 && f < Filas && c >= 0 && c < Columnas && Tablero[f, c] == jugador)
        {
            contador = contador + 1;
            f = f + cfila;
            c = c + ccolumna;
        }
        return contador;
    }

    private bool Tablerolleno()
    {
        for (int c = 0; c < Columnas; c++)
        {
            if (Tablero[0, c] == 0)
            {
                return false;
            }
        }
        return true;
    }

    public bool ColumnaLlena(int columna)
    {
        return Tablero[0, columna] != 0;
    }

    public void InsertarFichaDesdeBD(int columna, int jugador)
    {
        for (int fila = Filas - 1; fila >= 0; fila--)
        {
            if (Tablero[fila, columna] == 0)
            {
                Tablero[fila, columna] = jugador;
                return;
            }
        }
    }

}
