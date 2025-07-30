-- ========================================
-- Creación de la base de datos
-- ========================================
CREATE DATABASE Connect4DB;
GO
USE Connect4DB;
GO
-- ========================================
-- Tabla: jugadores
-- ========================================
CREATE TABLE jugadores (
   id BIGINT IDENTITY(1,1) PRIMARY KEY,
   cedula INT NOT NULL UNIQUE,
   nombre NVARCHAR(100) NOT NULL UNIQUE,
   apellido NVARCHAR(100) NOT NULL,
   partidas_ganadas INT DEFAULT 0,
   partidas_perdidas INT DEFAULT 0,
   partidas_empatadas INT DEFAULT 0,
   marcador AS (partidas_ganadas - partidas_perdidas) PERSISTED,
   fecha_creacion DATETIME2 DEFAULT GETDATE(),
   CONSTRAINT chk_partidas_ganadas CHECK (partidas_ganadas >= 0),
   CONSTRAINT chk_partidas_perdidas CHECK (partidas_perdidas >= 0),
   CONSTRAINT chk_partidas_empatadas CHECK (partidas_empatadas >= 0)
);
GO
-- ========================================
-- Tabla: partidas
-- ========================================
CREATE TABLE partidas (
   id INT IDENTITY(1,1) PRIMARY KEY,
   jugador1_id BIGINT NOT NULL,
   jugador2_id BIGINT NOT NULL,
   estado NVARCHAR(20) DEFAULT 'en_progreso' CHECK (estado IN ('en_progreso', 'finalizada')),
   resultado NVARCHAR(20) NULL CHECK (resultado IN ('jugador1', 'jugador2', 'empate')),
   turno_actual TINYINT DEFAULT 1 CHECK (turno_actual IN (1, 2)),
   fecha_creacion DATETIME2 DEFAULT GETDATE(),
   fecha_finalizacion DATETIME2 NULL,
   CONSTRAINT fk_partidas_jugador1 FOREIGN KEY (jugador1_id) REFERENCES jugadores(id),
   CONSTRAINT fk_partidas_jugador2 FOREIGN KEY (jugador2_id) REFERENCES jugadores(id),
   CONSTRAINT chk_jugadores_diferentes CHECK (jugador1_id != jugador2_id)
);
GO
-- Índice para buscar partidas por estado
CREATE INDEX idx_partidas_estado ON partidas (estado);
GO
-- ========================================
-- Tabla: movimientos
-- ========================================
CREATE TABLE movimientos (
   id INT IDENTITY(1,1) PRIMARY KEY,
   partida_id INT NOT NULL,
   jugador TINYINT NOT NULL CHECK (jugador IN (1, 2)),
   columna CHAR(1) NOT NULL CHECK (columna IN ('A', 'B', 'C', 'D', 'E', 'F', 'G')),
   fila TINYINT NOT NULL,
   orden_movimiento INT NOT NULL,
   CONSTRAINT fk_movimientos_partida FOREIGN KEY (partida_id) REFERENCES partidas(id) ON DELETE CASCADE
);
GO
-- ========================================
-- Vista: escalafón de jugadores
-- ========================================
CREATE VIEW escalafon_jugadores AS
SELECT
   id,
   cedula,
   nombre,
   apellido,
   marcador,
   partidas_ganadas,
   partidas_perdidas,
   partidas_empatadas,
   (partidas_ganadas + partidas_perdidas + partidas_empatadas) AS total_partidas
FROM jugadores;
GO
-- ========================================
-- Vista: partidas con nombres de jugadores
-- ========================================
CREATE VIEW vista_partidas AS
SELECT
   p.id,
   p.jugador1_id,
   CONCAT(j1.nombre, ' ', j1.apellido) AS jugador1_nombre,
   p.jugador2_id,
   CONCAT(j2.nombre, ' ', j2.apellido) AS jugador2_nombre,
   p.estado,
   p.resultado,
   p.turno_actual,
   p.fecha_creacion,
   p.fecha_finalizacion,
   CASE
       WHEN p.resultado = 'jugador1' THEN CONCAT(j1.nombre, ' ', j1.apellido)
       WHEN p.resultado = 'jugador2' THEN CONCAT(j2.nombre, ' ', j2.apellido)
       WHEN p.resultado = 'empate' THEN 'Empate'
       ELSE 'En progreso'
   END AS resultado_texto
FROM partidas p
INNER JOIN jugadores j1 ON p.jugador1_id = j1.id
INNER JOIN jugadores j2 ON p.jugador2_id = j2.id;
GO
-- ========================================
-- Procedimiento: insertar nuevo movimiento
-- ========================================
CREATE PROCEDURE sp_insertar_movimiento
   @partida_id INT,
   @jugador TINYINT,
   @columna CHAR(1)
AS
BEGIN
   SET NOCOUNT ON;
   DECLARE @fila TINYINT;
   DECLARE @orden_movimiento INT;
   BEGIN TRANSACTION;
   BEGIN TRY
       IF NOT EXISTS (SELECT 1 FROM partidas WHERE id = @partida_id AND estado = 'en_progreso')
       BEGIN
           RAISERROR('La partida no existe o ya finalizó', 16, 1);
           ROLLBACK TRANSACTION;
           RETURN;
       END
       SELECT @fila = ISNULL(MAX(fila), 0) + 1
       FROM movimientos
       WHERE partida_id = @partida_id AND columna = @columna;
       IF @fila > 6
       BEGIN
           RAISERROR('La columna está llena', 16, 1);
           ROLLBACK TRANSACTION;
           RETURN;
       END
       SELECT @orden_movimiento = ISNULL(MAX(orden_movimiento), 0) + 1
       FROM movimientos
       WHERE partida_id = @partida_id;
       INSERT INTO movimientos (partida_id, jugador, columna, fila, orden_movimiento)
       VALUES (@partida_id, @jugador, @columna, @fila, @orden_movimiento);
       UPDATE partidas
       SET turno_actual = CASE WHEN turno_actual = 1 THEN 2 ELSE 1 END
       WHERE id = @partida_id;
       COMMIT TRANSACTION;
       SELECT @fila AS fila_resultado;
   END TRY
   BEGIN CATCH
       ROLLBACK TRANSACTION;
       THROW;
   END CATCH
END
GO
-- ========================================
-- Procedimiento: crear nuevo jugador
-- ========================================
CREATE OR ALTER PROCEDURE sp_crear_jugador
   @cedula INT,
   @nombre NVARCHAR(100),
   @apellido NVARCHAR(100)
AS
BEGIN
   SET NOCOUNT ON;
   BEGIN TRY
       IF EXISTS (SELECT 1 FROM jugadores WHERE cedula = @cedula)
       BEGIN
           RAISERROR('Ya existe un jugador con esa cédula.', 16, 1);
           RETURN;
       END
       IF EXISTS (SELECT 1 FROM jugadores WHERE nombre = @nombre)
       BEGIN
           RAISERROR('El nombre de usuario ya está en uso.', 16, 1);
           RETURN;
       END
       INSERT INTO jugadores (cedula, nombre, apellido)
       VALUES (@cedula, @nombre, @apellido);
       SELECT SCOPE_IDENTITY() AS nuevo_jugador_id;
   END TRY
   BEGIN CATCH
       THROW;
   END CATCH
END
GO
---------------------------------------------------------------------------------------------------------------------------
USE Connect4DB;
GO
IF EXISTS (
   SELECT 1
   FROM sys.check_constraints
   WHERE name = 'chk_numero_jugador'
)
BEGIN
   ALTER TABLE jugadores DROP CONSTRAINT chk_numero_jugador;
END
GO
IF COL_LENGTH('jugadores', 'numero_jugador') IS NULL
BEGIN
   ALTER TABLE jugadores
   ADD numero_jugador TINYINT NOT NULL DEFAULT 1;
END
GO
ALTER TABLE jugadores
ADD CONSTRAINT chk_numero_jugador CHECK (numero_jugador IN (1, 2));
GO
CREATE OR ALTER PROCEDURE sp_crear_jugador
   @cedula INT,
   @nombre NVARCHAR(100),
   @apellido NVARCHAR(100),
   @numero_jugador TINYINT -- 1 o 2
AS
BEGIN
   SET NOCOUNT ON;
   BEGIN TRY
       IF @numero_jugador NOT IN (1, 2)
       BEGIN
           RAISERROR('El número de jugador debe ser 1 o 2.', 16, 1);
           RETURN;
       END
       IF EXISTS (SELECT 1 FROM jugadores WHERE cedula = @cedula)
       BEGIN
           RAISERROR('Ya existe un jugador con esa cédula.', 16, 1);
           RETURN;
       END
       IF EXISTS (SELECT 1 FROM jugadores WHERE nombre = @nombre)
       BEGIN
           RAISERROR('El nombre de usuario ya está en uso.', 16, 1);
           RETURN;
       END
       INSERT INTO jugadores (cedula, nombre, apellido, numero_jugador)
       VALUES (@cedula, @nombre, @apellido, @numero_jugador);
       SELECT SCOPE_IDENTITY() AS nuevo_jugador_id;
   END TRY
   BEGIN CATCH
       THROW;
   END CATCH
END
GO