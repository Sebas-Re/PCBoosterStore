-- Migración 5: se deja de guardar el código de seguridad (CVV) de las tarjetas.
-- Una tienda no debe almacenarlo y la compra no lo usa. Además, la clave de
-- Tarjetas lo incluía, así que la tabla se reconstruye con un id propio
-- (el rowid de cada fila, que es el id que ya usaba el sitio).

create table Tarjetas_v5(
	IdTarjeta_t integer primary key autoincrement,
	Id_TarjetaUsuario_t bigint not null,
	Titular_Tarjeta_t varchar(50) not null,
	Nombre_Tarjeta_t varchar(20) not null,
	Tipo_Tarjeta_t bit not null,
	Num_Tarjeta_t bigint not null,
	Fecha_Vencimiento_t date not null,
	constraint UQ_Tarjetas_Numero unique (Id_TarjetaUsuario_t, Num_Tarjeta_t),
	constraint FK_Tarjetas_Usuarios foreign key (Id_TarjetaUsuario_t) references Usuarios(IdUsuario_us)
);

-- Si un usuario tenía el mismo número guardado dos veces, queda la primera.
insert into Tarjetas_v5 (IdTarjeta_t, Id_TarjetaUsuario_t, Titular_Tarjeta_t, Nombre_Tarjeta_t, Tipo_Tarjeta_t, Num_Tarjeta_t, Fecha_Vencimiento_t)
	select rowid, Id_TarjetaUsuario_t, Titular_Tarjeta_t, Nombre_Tarjeta_t, Tipo_Tarjeta_t, Num_Tarjeta_t, Fecha_Vencimiento_t
	from Tarjetas
	where rowid in (select min(rowid) from Tarjetas group by Id_TarjetaUsuario_t, Num_Tarjeta_t);

drop table Tarjetas; -- se lleva el trigger tr_Tarjetas_Fecha_Vencimiento
alter table Tarjetas_v5 rename to Tarjetas;

create trigger tr_Tarjetas_Fecha_Vencimiento
before insert on Tarjetas
when new.Fecha_Vencimiento_t <= date('now', 'localtime')
begin
	select raise(abort, 'Ck_Tarjetas_Fecha_Vencimiento: la tarjeta está vencida');
end;

alter table Ventas drop column Codigo_Seguridad_ve;
