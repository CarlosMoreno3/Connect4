--Creacion de usuario para DB
CREATE LOGIN Connect4 WITH PASSWORD = 'Connect1234';
CREATE USER Connect4 FOR LOGIN Connect4;
ALTER SERVER ROLE sysadmin ADD MEMBER Connect4;

--Creacion de Base de Datos Connect4
CREATE DATABASE Connect4DB;

--
USE Connect4DB;
GO

--------------------------------------------
-- Tabla Jugadores
CREATE TABLE jugadores (
    id BIGINT IDENTITY(1,1) PRIMARY KEY,
    nombre NVARCHAR(100) NOT NULL,
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

-- Tabla de Partidas
CREATE TABLE partidas (
    id INT IDENTITY(1,1) PRIMARY KEY,
    jugador1_id BIGINT NOT NULL,
    jugador2_id BIGINT NOT NULL,
    estado NVARCHAR(20) DEFAULT 'en_progreso' CHECK (estado IN ('en_progreso', 'finalizada')),
    resultado NVARCHAR(20) NULL CHECK (resultado IN ('jugador1', 'jugador2', 'empate')),
    turno_actual TINYINT DEFAULT 1 CHECK (turno_actual IN (1, 2)),
    fecha_creacion DATETIME2 DEFAULT GETDATE(),
    fecha_finalizacion DATETIME2 NULL,
    
    -- llaves foraneas
    CONSTRAINT fk_partidas_jugador1 FOREIGN KEY (jugador1_id) REFERENCES jugadores(id),
    CONSTRAINT fk_partidas_jugador2 FOREIGN KEY (jugador2_id) REFERENCES jugadores(id),
    
    -- Restricciones
    CONSTRAINT chk_jugadores_diferentes CHECK (jugador1_id != jugador2_id)
);

-- indices ver partidas por el estado que tenga
CREATE INDEX idx_partidas_estado ON partidas (estado);
GO

-- Tabla de Movimientos
CREATE TABLE movimientos (
   id INT IDENTITY(1,1) PRIMARY KEY,
   partida_id INT NOT NULL,
   jugador TINYINT NOT NULL CHECK (jugador IN (1, 2)),
   columna CHAR(1) NOT NULL CHECK (columna IN ('A', 'B', 'C', 'D', 'E', 'F', 'G')),
   fila TINYINT NOT NULL CHECK (fila BETWEEN 1 AND 6),
   orden_movimiento INT NOT NULL
   
   -- llave foranea
   CONSTRAINT fk_movimientos_partida FOREIGN KEY (partida_id) REFERENCES partidas(id) ON DELETE CASCADE,
);
GO


-- Vista para el escalafón de jugadores
CREATE VIEW escalafon_jugadores AS
SELECT 
    id,
    nombre,
    apellido,
    correo,
    marcador,
    partidas_ganadas,
    partidas_perdidas,
    partidas_empatadas,
    (partidas_ganadas + partidas_perdidas + partidas_empatadas) as total_partidas
FROM jugadores;
GO

-- Vista para listar partidas con nombres de jugadores
CREATE VIEW vista_partidas AS
SELECT 
    p.id,
    p.jugador1_id,
    CONCAT(j1.nombre, ' ', j1.apellido) as jugador1_nombre,
    p.jugador2_id,
    CONCAT(j2.nombre, ' ', j2.apellido) as jugador2_nombre,
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
    END as resultado_texto
FROM partidas p
INNER JOIN jugadores j1 ON p.jugador1_id = j1.id
INNER JOIN jugadores j2 ON p.jugador2_id = j2.id;
GO

-- Procedimiento para insertar un nuevo movimiento
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
        -- Verifica que la partida está en progreso
        IF NOT EXISTS (SELECT 1 FROM partidas WHERE id = @partida_id AND estado = 'en_progreso')
        BEGIN
            RAISERROR('La partida no existe o ya finalizó', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END
        
        -- Encontrar la fila más baja disponible en la columna
        SELECT @fila = ISNULL(MAX(fila), 0) + 1
        FROM movimientos 
        WHERE partida_id = @partida_id AND columna = @columna;
        
        -- Verificar que la columna no está llena
        IF @fila > 6
        BEGIN
            RAISERROR('La columna está llena', 16, 1);
            ROLLBACK TRANSACTION;
            RETURN;
        END
        
        -- Obtener el siguiente número de orden
        SELECT @orden_movimiento = ISNULL(MAX(orden_movimiento), 0) + 1
        FROM movimientos 
        WHERE partida_id = @partida_id;
        
        -- Insertar el movimiento
        INSERT INTO movimientos (partida_id, jugador, columna, fila, orden_movimiento)
        VALUES (@partida_id, @jugador, @columna, @fila, @orden_movimiento);
        
        -- Actualizar el turno en la partida
        UPDATE partidas 
        SET turno_actual = CASE WHEN turno_actual = 1 THEN 2 ELSE 1 END
        WHERE id = @partida_id;
        
        COMMIT TRANSACTION;
        
        -- Devolver la fila donde cayó la ficha
        SELECT @fila as fila_resultado;
        
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

-- Procedimiento para crear un nuevo jugador (NUEVO)
CREATE PROCEDURE sp_crear_jugador
    @nombre NVARCHAR(100),
    @apellido NVARCHAR(100),
    @correo NVARCHAR(255),
    @contrasena NVARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        -- Verificar que el correo no exista
        IF EXISTS (SELECT 1 FROM jugadores WHERE correo = @correo)
        BEGIN
            RAISERROR('El correo electrónico ya está registrado', 16, 1);
            RETURN;
        END
        
        -- Insertar el nuevo jugador
        INSERT INTO jugadores (nombre, apellido, correo, contrasena)
        VALUES (@nombre, @apellido, @correo, @contrasena);
        
        -- Devolver el ID del jugador creado
        SELECT SCOPE_IDENTITY() as nuevo_jugador_id;
        
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO