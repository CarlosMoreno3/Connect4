using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Connect4.Models
{
    [Table("jugadores")]
    public class Jugador
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Required]
        [Column("cedula")]
        public int Cedula { get; set; }

        [Required]
        [StringLength(100)]
        [Column("nombre")]
        public string Nombre { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        [Column("apellido")]
        public string Apellido { get; set; } = string.Empty;

        [Column("numero_jugador")]
        public byte NumeroJugador { get; set; } = 1;  // 1 o 2 según corresponda

        [Column("partidas_ganadas")]
        public int PartidasGanadas { get; set; } = 0;

        [Column("partidas_perdidas")]
        public int PartidasPerdidas { get; set; } = 0;

        [Column("partidas_empatadas")]
        public int PartidasEmpatadas { get; set; } = 0;

        [Column("marcador")]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public int Marcador { get; set; }

        [Column("fecha_creacion")]
        public DateTime FechaCreacion { get; set; } = DateTime.Now;
    }

    [Table("partidas")]
    public class Partida
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("jugador1_id")]
        public long Jugador1Id { get; set; }

        [Required]
        [Column("jugador2_id")]
        public long Jugador2Id { get; set; }

        [StringLength(20)]
        [Column("estado")]
        public string Estado { get; set; } = "en_progreso";

        [StringLength(20)]
        [Column("resultado")]
        public string? Resultado { get; set; }

        [Column("turno_actual")]
        public byte TurnoActual { get; set; } = 1;

        [Column("fecha_creacion")]
        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        [Column("fecha_finalizacion")]
        public DateTime? FechaFinalizacion { get; set; }

        public virtual Jugador? Jugador1 { get; set; }
        public virtual Jugador? Jugador2 { get; set; }

        public ICollection<Movimiento>? Movimientos { get; set; }
    }

    [Table("movimientos")]
    public class Movimiento
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("partida_id")]
        public int PartidaId { get; set; }

        [Required]
        [Column("jugador")]
        public byte Jugador { get; set; }

        [Required]
        [StringLength(1)]
        [Column("columna")]
        public string Columna { get; set; } = string.Empty;

        [Required]
        [Column("fila")]
        public byte Fila { get; set; }

        [Required]
        [Column("orden_movimiento")]
        public int OrdenMovimiento { get; set; }
    }
}