-- Marca, descripción, especificaciones y garantía de los artículos de prueba.
-- Datos inventados (verosímiles) para mostrar el detalle del producto en el catálogo.
-- Especificaciones: una por línea, con el formato "Clave: Valor".

update Articulos set Marca = 'Deepcool', GarantiaMeses = 36, DescripcionLarga = 'Fuente de alimentación Deepcool de 700 W con certificación 80 Plus Bronze. Ideal para equipos gamer de gama media con una placa de video dedicada.',
	Especificaciones = 'Potencia: 700 W
Certificación: 80 Plus Bronze
Modularidad: No modular
Ventilador: 120 mm hidráulico
PFC: Activo
Conectores PCIe: 2 x 6+2 pines'
	where Codigo = 'fnt1';
update Articulos set Marca = 'Thermaltake', GarantiaMeses = 36, DescripcionLarga = 'Fuente de alimentación Thermaltake de 750 W con certificación 80 Plus Gold. Iluminación RGB direccionable y cables planos semi modulares para un armado prolijo.',
	Especificaciones = 'Potencia: 750 W
Certificación: 80 Plus Gold
Modularidad: Semi modular
Ventilador: 120 mm RGB
PFC: Activo
Conectores PCIe: 4 x 6+2 pines'
	where Codigo = 'fnt2';
update Articulos set Marca = 'Segotep', GarantiaMeses = 24, DescripcionLarga = 'Fuente de alimentación Segotep de 500 W con certificación 80 Plus White. Opción económica y confiable para equipos de oficina o gaming liviano.',
	Especificaciones = 'Potencia: 500 W
Certificación: 80 Plus White
Modularidad: No modular
Ventilador: 120 mm
PFC: Activo
Conectores PCIe: 1 x 6+2 pines'
	where Codigo = 'fnt3';
update Articulos set Marca = 'Seasonic', GarantiaMeses = 36, DescripcionLarga = 'Fuente de alimentación Seasonic de 500 W con certificación 80 Plus Bronze. La calidad de componentes de Seasonic en un modelo de entrada muy silencioso.',
	Especificaciones = 'Potencia: 500 W
Certificación: 80 Plus Bronze
Modularidad: No modular
Ventilador: 120 mm silencioso
PFC: Activo
Conectores PCIe: 2 x 6+2 pines'
	where Codigo = 'fnt4';
update Articulos set Marca = 'ASUS', GarantiaMeses = 36, DescripcionLarga = 'Fuente de alimentación ASUS de 750 W con certificación 80 Plus Gold. Línea ROG Strix con disipadores ROG Heatsink y modo 0 dB en baja carga.',
	Especificaciones = 'Potencia: 750 W
Certificación: 80 Plus Gold
Modularidad: Full modular
Ventilador: 135 mm Axial-tech
PFC: Activo
Conectores PCIe: 4 x 6+2 pines'
	where Codigo = 'fnt5';
update Articulos set Marca = 'ASUS', GarantiaMeses = 36, DescripcionLarga = 'Fuente de alimentación ASUS de 550 W con certificación 80 Plus Bronze. Línea TUF Gaming con capacitores de grado militar y recubrimiento PCB.',
	Especificaciones = 'Potencia: 550 W
Certificación: 80 Plus Bronze
Modularidad: No modular
Ventilador: 135 mm doble rodamiento
PFC: Activo
Conectores PCIe: 2 x 6+2 pines'
	where Codigo = 'fnt6';
update Articulos set Marca = 'Gigabyte', GarantiaMeses = 36, DescripcionLarga = 'Fuente de alimentación Gigabyte de 550 W con certificación 80 Plus Bronze. Protecciones OVP, OPP, SCP y UVP para cuidar todos tus componentes.',
	Especificaciones = 'Potencia: 550 W
Certificación: 80 Plus Bronze
Modularidad: No modular
Ventilador: 120 mm
PFC: Activo
Conectores PCIe: 2 x 6+2 pines'
	where Codigo = 'fnt7';
update Articulos set Marca = 'be quiet!', GarantiaMeses = 36, DescripcionLarga = 'Fuente de alimentación be quiet! de 600 W con certificación 80 Plus Gold. Pensada para equipos silenciosos: funcionamiento casi inaudible incluso a plena carga.',
	Especificaciones = 'Potencia: 600 W
Certificación: 80 Plus Gold
Modularidad: Semi modular
Ventilador: 135 mm Silent Wings
PFC: Activo
Conectores PCIe: 2 x 6+2 pines'
	where Codigo = 'fnt8';
update Articulos set Marca = 'Redragon', GarantiaMeses = 36, DescripcionLarga = 'Fuente de alimentación Redragon de 800 W con certificación 80 Plus Gold. Potencia de sobra para placas de video de alta gama y overclock.',
	Especificaciones = 'Potencia: 800 W
Certificación: 80 Plus Gold
Modularidad: Full modular
Ventilador: 140 mm
PFC: Activo
Conectores PCIe: 4 x 6+2 pines'
	where Codigo = 'fnt9';
update Articulos set Marca = 'Gamemax', GarantiaMeses = 12, DescripcionLarga = 'Gabinete Mid Tower de Gamemax. Buena ventilación y un diseño gamer a un precio accesible.',
	Especificaciones = 'Formato: Mid Tower
Placas compatibles: ATX, mATX, ITX
Ventiladores incluidos: 1 x 120 mm RGB
Panel lateral: Acrílico
Largo máx. de placa de video: 320 mm
Puertos frontales: 2 x USB 3.0, audio'
	where Codigo = 'gb1';
update Articulos set Marca = 'Aerocool', GarantiaMeses = 12, DescripcionLarga = 'Gabinete Mid Tower de Aerocool. Frente con iluminación RGB y filtro antipolvo removible.',
	Especificaciones = 'Formato: Mid Tower
Placas compatibles: ATX, mATX, ITX
Ventiladores incluidos: 1 x 120 mm
Panel lateral: Acrílico
Largo máx. de placa de video: 330 mm
Puertos frontales: 2 x USB 3.0, audio'
	where Codigo = 'gb2';
update Articulos set Marca = 'Deepcool', GarantiaMeses = 12, DescripcionLarga = 'Gabinete Mid Tower de Deepcool. Frente de malla para máximo flujo de aire.',
	Especificaciones = 'Formato: Mid Tower
Placas compatibles: ATX, mATX, ITX
Ventiladores incluidos: 3 x 120 mm ARGB
Panel lateral: Vidrio templado
Largo máx. de placa de video: 370 mm
Puertos frontales: 2 x USB 3.0, audio'
	where Codigo = 'gb3';
update Articulos set Marca = 'Deepcool', GarantiaMeses = 12, DescripcionLarga = 'Gabinete Mid Tower de Deepcool. Línea MATREXX: interior amplio con gestión de cables detrás de la bandeja.',
	Especificaciones = 'Formato: Mid Tower
Placas compatibles: E-ATX, ATX, mATX
Ventiladores incluidos: 1 x 120 mm
Panel lateral: Vidrio templado
Largo máx. de placa de video: 370 mm
Puertos frontales: 2 x USB 3.0, audio'
	where Codigo = 'gb4';
update Articulos set Marca = 'Kolink', GarantiaMeses = 12, DescripcionLarga = 'Gabinete Mid Tower de Kolink. Bastion: cuatro ventiladores ARGB y controlador incluidos.',
	Especificaciones = 'Formato: Mid Tower
Placas compatibles: ATX, mATX, ITX
Ventiladores incluidos: 4 x 120 mm ARGB
Panel lateral: Vidrio templado
Largo máx. de placa de video: 350 mm
Puertos frontales: 2 x USB 3.0, audio'
	where Codigo = 'gb5';
update Articulos set Marca = 'Cooler Master', GarantiaMeses = 12, DescripcionLarga = 'Gabinete Mid Tower de Cooler Master. Línea MasterBox con frente de malla y soporte para radiador de 360 mm.',
	Especificaciones = 'Formato: Mid Tower
Placas compatibles: ATX, mATX, ITX
Ventiladores incluidos: 2 x 120 mm ARGB
Panel lateral: Vidrio templado
Largo máx. de placa de video: 360 mm
Puertos frontales: 2 x USB 3.0, audio'
	where Codigo = 'gb6';
update Articulos set Marca = 'Cougar', GarantiaMeses = 12, DescripcionLarga = 'Gabinete Mid Tower de Cougar. Diseño angular característico de Cougar con iluminación ARGB.',
	Especificaciones = 'Formato: Mid Tower
Placas compatibles: ATX, mATX, ITX
Ventiladores incluidos: 3 x 120 mm ARGB
Panel lateral: Vidrio templado
Largo máx. de placa de video: 350 mm
Puertos frontales: 2 x USB 3.0, audio'
	where Codigo = 'gb7';
update Articulos set Marca = 'ASUS', GarantiaMeses = 12, DescripcionLarga = 'Gabinete Full Tower de ASUS. Línea ROG: el gabinete premium para builds de máxima gama.',
	Especificaciones = 'Formato: Full Tower
Placas compatibles: E-ATX, ATX, mATX
Ventiladores incluidos: 4 x 140 mm
Panel lateral: Vidrio templado
Largo máx. de placa de video: 450 mm
Puertos frontales: 2 x USB 3.0, audio'
	where Codigo = 'gb8';
update Articulos set Marca = 'ASUS', GarantiaMeses = 12, DescripcionLarga = 'Gabinete Mid Tower de ASUS. TUF Gaming: estructura reforzada y frente con detalles en malla.',
	Especificaciones = 'Formato: Mid Tower
Placas compatibles: ATX, mATX, ITX
Ventiladores incluidos: 1 x 120 mm
Panel lateral: Vidrio templado
Largo máx. de placa de video: 365 mm
Puertos frontales: 2 x USB 3.0, audio'
	where Codigo = 'gb9';
update Articulos set Marca = 'Seagate', GarantiaMeses = 24, DescripcionLarga = 'Disco rígido Seagate de 8 TB pensado para almacenamiento masivo. Ideal para guardar juegos, fotos y backups.',
	Especificaciones = 'Capacidad: 8 TB
Velocidad: 5400 RPM
Caché: 256 MB
Interfaz: SATA III 6 Gb/s
Formato: 3,5"
Uso: Almacenamiento masivo'
	where Codigo = 'hdd1';
update Articulos set Marca = 'Seagate', GarantiaMeses = 12, DescripcionLarga = 'Disco rígido Seagate de 3 TB pensado para escritorio. Ideal para guardar juegos, fotos y backups.',
	Especificaciones = 'Capacidad: 3 TB
Velocidad: 5400 RPM
Caché: 256 MB
Interfaz: SATA III 6 Gb/s
Formato: 3,5"
Uso: Escritorio'
	where Codigo = 'hdd2';
update Articulos set Marca = 'Toshiba', GarantiaMeses = 12, DescripcionLarga = 'Disco rígido Toshiba de 4 TB pensado para escritorio. Ideal para guardar juegos, fotos y backups.',
	Especificaciones = 'Capacidad: 4 TB
Velocidad: 7200 RPM
Caché: 128 MB
Interfaz: SATA III 6 Gb/s
Formato: 3,5"
Uso: Escritorio'
	where Codigo = 'hdd3';
update Articulos set Marca = 'Western Digital', GarantiaMeses = 24, DescripcionLarga = 'Disco rígido Western Digital de 8 TB pensado para nas. Ideal para guardar juegos, fotos y backups.',
	Especificaciones = 'Capacidad: 8 TB
Velocidad: 5640 RPM
Caché: 256 MB
Interfaz: SATA III 6 Gb/s
Formato: 3,5"
Uso: NAS'
	where Codigo = 'hdd4';
update Articulos set Marca = 'Western Digital', GarantiaMeses = 12, DescripcionLarga = 'Disco rígido Western Digital de 3 TB pensado para escritorio. Ideal para guardar juegos, fotos y backups.',
	Especificaciones = 'Capacidad: 3 TB
Velocidad: 5400 RPM
Caché: 256 MB
Interfaz: SATA III 6 Gb/s
Formato: 3,5"
Uso: Escritorio'
	where Codigo = 'hdd5';
update Articulos set Marca = 'Seagate', GarantiaMeses = 24, DescripcionLarga = 'Disco rígido Seagate de 14 TB pensado para nas. Ideal para guardar juegos, fotos y backups.',
	Especificaciones = 'Capacidad: 14 TB
Velocidad: 7200 RPM
Caché: 256 MB
Interfaz: SATA III 6 Gb/s
Formato: 3,5"
Uso: NAS'
	where Codigo = 'hdd6';
update Articulos set Marca = 'Western Digital', GarantiaMeses = 12, DescripcionLarga = 'Disco rígido Western Digital de 6 TB pensado para escritorio. Ideal para guardar juegos, fotos y backups.',
	Especificaciones = 'Capacidad: 6 TB
Velocidad: 5400 RPM
Caché: 256 MB
Interfaz: SATA III 6 Gb/s
Formato: 3,5"
Uso: Escritorio'
	where Codigo = 'hdd8';
update Articulos set Marca = 'Western Digital', GarantiaMeses = 24, DescripcionLarga = 'Disco rígido Western Digital de 6 TB pensado para videovigilancia. Ideal para guardar juegos, fotos y backups.',
	Especificaciones = 'Capacidad: 6 TB
Velocidad: 5400 RPM
Caché: 256 MB
Interfaz: SATA III 6 Gb/s
Formato: 3,5"
Uso: Videovigilancia'
	where Codigo = 'hdd9';
update Articulos set Marca = 'OLOy', GarantiaMeses = 36, DescripcionLarga = 'Memoria RAM OLOy DDR4 de 64 GB (2 x 32 GB) a 3200 MHz. Mejora la fluidez del sistema y de los juegos.',
	Especificaciones = 'Capacidad: 64 GB (2 x 32 GB)
Tipo: DDR4 UDIMM
Frecuencia: 3200 MHz
Latencia: CL16
Voltaje: 1,35 V
Disipador: Aluminio'
	where Codigo = 'mr1';
update Articulos set Marca = 'Geil', GarantiaMeses = 120, DescripcionLarga = 'Memoria RAM Geil DDR4 de 8 GB a 3200 MHz. Mejora la fluidez del sistema y de los juegos.',
	Especificaciones = 'Capacidad: 8 GB
Tipo: DDR4 UDIMM
Frecuencia: 3200 MHz
Latencia: CL16
Voltaje: 1,35 V
Disipador: Aluminio'
	where Codigo = 'mr2';
update Articulos set Marca = 'Team', GarantiaMeses = 120, DescripcionLarga = 'Memoria RAM Team DDR4 de 8 GB a 3200 MHz. Mejora la fluidez del sistema y de los juegos.',
	Especificaciones = 'Capacidad: 8 GB
Tipo: DDR4 UDIMM
Frecuencia: 3200 MHz
Latencia: CL16
Voltaje: 1,35 V
Disipador: T-Force Vulcan Z'
	where Codigo = 'mr3';
update Articulos set Marca = 'LeiL', GarantiaMeses = 36, DescripcionLarga = 'Memoria RAM LeiL DDR4 de 8 GB a 2666 MHz. Mejora la fluidez del sistema y de los juegos.',
	Especificaciones = 'Capacidad: 8 GB
Tipo: DDR4 UDIMM
Frecuencia: 2666 MHz
Latencia: CL19
Voltaje: 1,35 V
Disipador: Sin disipador'
	where Codigo = 'mr6';
update Articulos set Marca = 'Adata', GarantiaMeses = 120, DescripcionLarga = 'Memoria RAM Adata DDR4 de 16 GB a 3200 MHz. Mejora la fluidez del sistema y de los juegos.',
	Especificaciones = 'Capacidad: 16 GB
Tipo: DDR4 UDIMM
Frecuencia: 3200 MHz
Latencia: CL16
Voltaje: 1,35 V
Disipador: XPG Gammix'
	where Codigo = 'mr7';
update Articulos set Marca = 'PNY', GarantiaMeses = 36, DescripcionLarga = 'Memoria RAM PNY DDR4 de 8 GB a 2666 MHz. Mejora la fluidez del sistema y de los juegos.',
	Especificaciones = 'Capacidad: 8 GB
Tipo: DDR4 UDIMM
Frecuencia: 2666 MHz
Latencia: CL19
Voltaje: 1,35 V
Disipador: Sin disipador'
	where Codigo = 'mr8';
update Articulos set Marca = 'Team', GarantiaMeses = 120, DescripcionLarga = 'Memoria RAM Team DDR4 de 16 GB a 3600 MHz. Mejora la fluidez del sistema y de los juegos.',
	Especificaciones = 'Capacidad: 16 GB
Tipo: DDR4 UDIMM
Frecuencia: 3600 MHz
Latencia: CL18
Voltaje: 1,35 V
Disipador: T-Force Delta RGB'
	where Codigo = 'mr9';
update Articulos set Marca = 'Nvidia', GarantiaMeses = 36, DescripcionLarga = 'Placa de video GeForce GTX 1050 Ti con 4 GB GDDR5. Para jugar en 1080p títulos competitivos sin necesidad de conector de alimentación extra.',
	Especificaciones = 'GPU: GeForce GTX 1050 Ti
Memoria: 4 GB GDDR5
Interfaz: PCI Express 3.0 x16
Salidas: HDMI, DisplayPort, DVI
Consumo: 75 W
Fuente recomendada: 300 W'
	where Codigo = 'pdv1';
update Articulos set Marca = 'Nvidia', GarantiaMeses = 36, DescripcionLarga = 'Placa de video GeForce GT 710 con 1 GB DDR3. Placa de bajo perfil para sumar salidas de video a equipos de oficina.',
	Especificaciones = 'GPU: GeForce GT 710
Memoria: 1 GB DDR3
Interfaz: PCI Express 3.0 x16
Salidas: HDMI, VGA, DVI
Consumo: 19 W
Fuente recomendada: 300 W'
	where Codigo = 'pdv2';
update Articulos set Marca = 'Nvidia', GarantiaMeses = 36, DescripcionLarga = 'Placa de video GeForce GT 730 con 2 GB GDDR5. Solución económica para multimedia y reproducción 4K.',
	Especificaciones = 'GPU: GeForce GT 730
Memoria: 2 GB GDDR5
Interfaz: PCI Express 3.0 x16
Salidas: HDMI, VGA, DVI
Consumo: 38 W
Fuente recomendada: 300 W'
	where Codigo = 'pdv3';
update Articulos set Marca = 'Nvidia', GarantiaMeses = 36, DescripcionLarga = 'Placa de video GeForce RTX 3060 con 8 GB GDDR6. Ray tracing y DLSS para jugar en 1080p y 1440p con todo al máximo.',
	Especificaciones = 'GPU: GeForce RTX 3060
Memoria: 8 GB GDDR6
Interfaz: PCI Express 4.0 x16
Salidas: 3 x DisplayPort, HDMI
Consumo: 170 W
Fuente recomendada: 550 W'
	where Codigo = 'pdv4';
update Articulos set Marca = 'Nvidia', GarantiaMeses = 36, DescripcionLarga = 'Placa de video GeForce RTX 3060 Ti con 8 GB GDDR6. Rendimiento de gama alta para 1440p con ray tracing.',
	Especificaciones = 'GPU: GeForce RTX 3060 Ti
Memoria: 8 GB GDDR6
Interfaz: PCI Express 4.0 x16
Salidas: 3 x DisplayPort, HDMI
Consumo: 200 W
Fuente recomendada: 600 W'
	where Codigo = 'pdv5';
update Articulos set Marca = 'Nvidia', GarantiaMeses = 36, DescripcionLarga = 'Placa de video GeForce GTX 1660 Ti con 6 GB GDDR6. Excelente relación precio rendimiento para 1080p a altos FPS.',
	Especificaciones = 'GPU: GeForce GTX 1660 Ti
Memoria: 6 GB GDDR6
Interfaz: PCI Express 3.0 x16
Salidas: DisplayPort, HDMI, DVI
Consumo: 120 W
Fuente recomendada: 450 W'
	where Codigo = 'pdv6';
update Articulos set Marca = 'ASUS', GarantiaMeses = 36, DescripcionLarga = 'Placa de video GeForce RTX 3070 con 8 GB GDDR6. Línea TUF Gaming con triple ventilador Axial-tech y backplate metálico.',
	Especificaciones = 'GPU: GeForce RTX 3070
Memoria: 8 GB GDDR6
Interfaz: PCI Express 4.0 x16
Salidas: 3 x DisplayPort, 2 x HDMI
Consumo: 220 W
Fuente recomendada: 650 W'
	where Codigo = 'pdv7';
update Articulos set Marca = 'Nvidia', GarantiaMeses = 36, DescripcionLarga = 'Placa de video GeForce RTX 3060 con 12 GB GDDR6. 12 GB de memoria para juegos exigentes y edición de video.',
	Especificaciones = 'GPU: GeForce RTX 3060
Memoria: 12 GB GDDR6
Interfaz: PCI Express 4.0 x16
Salidas: 3 x DisplayPort, HDMI
Consumo: 170 W
Fuente recomendada: 550 W'
	where Codigo = 'pdv8';
update Articulos set Marca = 'Nvidia', GarantiaMeses = 36, DescripcionLarga = 'Placa de video GeForce GT 710 con 1 GB DDR3. Pasiva y silenciosa: ideal para HTPC y puestos de trabajo.',
	Especificaciones = 'GPU: GeForce GT 710
Memoria: 1 GB DDR3
Interfaz: PCI Express 3.0 x16
Salidas: HDMI, VGA, DVI
Consumo: 19 W
Fuente recomendada: 300 W'
	where Codigo = 'pdv9';
update Articulos set Marca = 'MSI', GarantiaMeses = 36, DescripcionLarga = 'Placa madre MSI B450 para AM4. Línea PRO con BIOS Flash Back para actualizar sin procesador.',
	Especificaciones = 'Socket: AM4
Chipset: B450
Formato: Micro ATX
Memoria: 4 x DDR4 hasta 64 GB
Slots M.2: 1
Red: Gigabit Ethernet'
	where Codigo = 'pm1';
update Articulos set Marca = 'ASUS', GarantiaMeses = 36, DescripcionLarga = 'Placa madre ASUS H410 para LGA 1200. Base sólida para procesadores Intel de 10.ª generación.',
	Especificaciones = 'Socket: LGA 1200
Chipset: H410
Formato: Micro ATX
Memoria: 2 x DDR4 hasta 64 GB
Slots M.2: 1
Red: Gigabit Ethernet'
	where Codigo = 'pm2';
update Articulos set Marca = 'AMD', GarantiaMeses = 36, DescripcionLarga = 'Placa madre AMD A68H para FM2+. Combo con procesador AMD A6 incluido: listo para armar un equipo de oficina.',
	Especificaciones = 'Socket: FM2+
Chipset: A68H
Formato: Micro ATX
Memoria: 2 x DDR3 hasta 32 GB
Slots M.2: 0
Red: Gigabit Ethernet'
	where Codigo = 'pm3';
update Articulos set Marca = 'ASUS', GarantiaMeses = 36, DescripcionLarga = 'Placa madre ASUS A320 para AM4. Línea Prime: la opción de entrada para Ryzen.',
	Especificaciones = 'Socket: AM4
Chipset: A320
Formato: Micro ATX
Memoria: 2 x DDR4 hasta 32 GB
Slots M.2: 1
Red: Gigabit Ethernet'
	where Codigo = 'pm4';
update Articulos set Marca = 'ASUS', GarantiaMeses = 36, DescripcionLarga = 'Placa madre ASUS H410 para LGA 1200. Protección contra sobretensiones y componentes de larga duración.',
	Especificaciones = 'Socket: LGA 1200
Chipset: H410
Formato: Micro ATX
Memoria: 2 x DDR4 hasta 64 GB
Slots M.2: 1
Red: Gigabit Ethernet'
	where Codigo = 'pm5';
update Articulos set Marca = 'ASRock', GarantiaMeses = 36, DescripcionLarga = 'Placa madre ASRock A320 para AM4. Compacta y económica, compatible con Ryzen 1000 a 3000.',
	Especificaciones = 'Socket: AM4
Chipset: A320
Formato: Micro ATX
Memoria: 2 x DDR4 hasta 32 GB
Slots M.2: 1
Red: Gigabit Ethernet'
	where Codigo = 'pm6';
update Articulos set Marca = 'ASRock', GarantiaMeses = 36, DescripcionLarga = 'Placa madre ASRock B450 para AM4. Soporte para memorias rápidas y overclock en procesadores Ryzen.',
	Especificaciones = 'Socket: AM4
Chipset: B450
Formato: Micro ATX
Memoria: 4 x DDR4 hasta 64 GB
Slots M.2: 1
Red: Gigabit Ethernet'
	where Codigo = 'pm8';
update Articulos set Marca = 'Gigabyte', GarantiaMeses = 36, DescripcionLarga = 'Placa madre Gigabyte B550 para AM4. Línea AORUS con PCIe 4.0 y VRM reforzado para Ryzen 5000.',
	Especificaciones = 'Socket: AM4
Chipset: B550
Formato: ATX
Memoria: 4 x DDR4 hasta 128 GB
Slots M.2: 2
Red: Gigabit Ethernet'
	where Codigo = 'pm9';
update Articulos set Marca = 'Intel', GarantiaMeses = 36, DescripcionLarga = 'Procesador Intel Core i5-10400. Gran equilibrio para gaming y productividad. Incluye cooler.',
	Especificaciones = 'Modelo: Core i5-10400
Núcleos / hilos: 6 / 12
Frecuencia: 2,9 GHz (turbo 4,3 GHz)
Socket: LGA 1200
Caché: 12 MB
TDP: 65 W'
	where Codigo = 'ps1';
update Articulos set Marca = 'AMD', GarantiaMeses = 36, DescripcionLarga = 'Procesador AMD Ryzen 7 5700X. Ocho núcleos Zen 3 para jugar, transmitir y editar al mismo tiempo.',
	Especificaciones = 'Modelo: Ryzen 7 5700X
Núcleos / hilos: 8 / 16
Frecuencia: 3,4 GHz (turbo 4,6 GHz)
Socket: AM4
Caché: 32 MB
TDP: 65 W'
	where Codigo = 'ps2';
update Articulos set Marca = 'AMD', GarantiaMeses = 36, DescripcionLarga = 'Procesador AMD Athlon 3000G. Con gráficos Radeon Vega integrados: no necesita placa de video.',
	Especificaciones = 'Modelo: Athlon 3000G
Núcleos / hilos: 2 / 4
Frecuencia: 3,5 GHz
Socket: AM4
Caché: 4 MB
TDP: 35 W'
	where Codigo = 'ps3';
update Articulos set Marca = 'Intel', GarantiaMeses = 36, DescripcionLarga = 'Procesador Intel Core i9-10900. Diez núcleos para tareas pesadas de render y gaming de alto rendimiento.',
	Especificaciones = 'Modelo: Core i9-10900
Núcleos / hilos: 10 / 20
Frecuencia: 2,8 GHz (turbo 5,2 GHz)
Socket: LGA 1200
Caché: 20 MB
TDP: 65 W'
	where Codigo = 'ps4';
update Articulos set Marca = 'Intel', GarantiaMeses = 36, DescripcionLarga = 'Procesador Intel Core i3-10100. Ideal para equipos de oficina y gaming de entrada.',
	Especificaciones = 'Modelo: Core i3-10100
Núcleos / hilos: 4 / 8
Frecuencia: 3,6 GHz (turbo 4,3 GHz)
Socket: LGA 1200
Caché: 6 MB
TDP: 65 W'
	where Codigo = 'ps5';
update Articulos set Marca = 'Intel', GarantiaMeses = 36, DescripcionLarga = 'Procesador Intel Core i9-10900. Versión box con disipador Intel incluido.',
	Especificaciones = 'Modelo: Core i9-10900
Núcleos / hilos: 10 / 20
Frecuencia: 2,8 GHz (turbo 5,2 GHz)
Socket: LGA 1200
Caché: 20 MB
TDP: 65 W'
	where Codigo = 'ps6';
update Articulos set Marca = 'AMD', GarantiaMeses = 36, DescripcionLarga = 'Procesador AMD Ryzen 7 3800XT. Frecuencias más altas de la serie 3000. No incluye cooler.',
	Especificaciones = 'Modelo: Ryzen 7 3800XT
Núcleos / hilos: 8 / 16
Frecuencia: 3,9 GHz (turbo 4,7 GHz)
Socket: AM4
Caché: 32 MB
TDP: 105 W'
	where Codigo = 'ps7';
update Articulos set Marca = 'Intel', GarantiaMeses = 36, DescripcionLarga = 'Procesador Intel Core i9-10900KA. Edición especial desbloqueada para overclock. No incluye cooler.',
	Especificaciones = 'Modelo: Core i9-10900KA
Núcleos / hilos: 10 / 20
Frecuencia: 3,7 GHz (turbo 5,3 GHz)
Socket: LGA 1200
Caché: 20 MB
TDP: 125 W'
	where Codigo = 'ps8';
update Articulos set Marca = 'Intel', GarantiaMeses = 36, DescripcionLarga = 'Procesador Intel Core i3-9100. Procesador de 9.ª generación con gráficos Intel UHD 630.',
	Especificaciones = 'Modelo: Core i3-9100
Núcleos / hilos: 4 / 4
Frecuencia: 3,6 GHz (turbo 4,2 GHz)
Socket: LGA 1151
Caché: 6 MB
TDP: 65 W'
	where Codigo = 'ps9';
update Articulos set Marca = 'Kingston', GarantiaMeses = 36, DescripcionLarga = 'Disco sólido Kingston de 1 TB. Reduce los tiempos de arranque y de carga de forma notable frente a un disco rígido.',
	Especificaciones = 'Capacidad: 1 TB
Formato: M.2 2280
Interfaz: PCIe 3.0 x4 NVMe
Lectura secuencial: 3500 MB/s
Escritura secuencial: 2100 MB/s
Durabilidad: 600 TBW'
	where Codigo = 'sdd1';
update Articulos set Marca = 'Team', GarantiaMeses = 36, DescripcionLarga = 'Disco sólido Team de 1 TB. Reduce los tiempos de arranque y de carga de forma notable frente a un disco rígido.',
	Especificaciones = 'Capacidad: 1 TB
Formato: 2,5"
Interfaz: SATA III
Lectura secuencial: 560 MB/s
Escritura secuencial: 500 MB/s
Durabilidad: 560 TBW'
	where Codigo = 'sdd2';
update Articulos set Marca = 'Team', GarantiaMeses = 36, DescripcionLarga = 'Disco sólido Team de 128 GB. Reduce los tiempos de arranque y de carga de forma notable frente a un disco rígido.',
	Especificaciones = 'Capacidad: 128 GB
Formato: 2,5"
Interfaz: SATA III
Lectura secuencial: 500 MB/s
Escritura secuencial: 400 MB/s
Durabilidad: 60 TBW'
	where Codigo = 'sdd3';
update Articulos set Marca = 'Adata', GarantiaMeses = 36, DescripcionLarga = 'Disco sólido Adata de 1,92 TB. Reduce los tiempos de arranque y de carga de forma notable frente a un disco rígido.',
	Especificaciones = 'Capacidad: 1,92 TB
Formato: 2,5"
Interfaz: SATA III
Lectura secuencial: 560 MB/s
Escritura secuencial: 520 MB/s
Durabilidad: 1000 TBW'
	where Codigo = 'sdd4';
update Articulos set Marca = 'Adata', GarantiaMeses = 36, DescripcionLarga = 'Disco sólido Adata de 1 TB. Reduce los tiempos de arranque y de carga de forma notable frente a un disco rígido.',
	Especificaciones = 'Capacidad: 1 TB
Formato: M.2 2280
Interfaz: PCIe 3.0 x4 NVMe
Lectura secuencial: 2100 MB/s
Escritura secuencial: 1500 MB/s
Durabilidad: 640 TBW'
	where Codigo = 'sdd5';
update Articulos set Marca = 'Gigabyte', GarantiaMeses = 36, DescripcionLarga = 'Disco sólido Gigabyte de 1 TB. Reduce los tiempos de arranque y de carga de forma notable frente a un disco rígido.',
	Especificaciones = 'Capacidad: 1 TB
Formato: M.2 2280
Interfaz: PCIe 3.0 x4 NVMe
Lectura secuencial: 2500 MB/s
Escritura secuencial: 2100 MB/s
Durabilidad: 600 TBW'
	where Codigo = 'sdd6';
update Articulos set Marca = 'Team', GarantiaMeses = 36, DescripcionLarga = 'Disco sólido Team de 500 GB. Reduce los tiempos de arranque y de carga de forma notable frente a un disco rígido.',
	Especificaciones = 'Capacidad: 500 GB
Formato: 2,5"
Interfaz: SATA III
Lectura secuencial: 550 MB/s
Escritura secuencial: 500 MB/s
Durabilidad: 300 TBW'
	where Codigo = 'sdd7';
update Articulos set Marca = 'Western Digital', GarantiaMeses = 36, DescripcionLarga = 'Disco sólido Western Digital de 2 TB. Reduce los tiempos de arranque y de carga de forma notable frente a un disco rígido.',
	Especificaciones = 'Capacidad: 2 TB
Formato: M.2 2280
Interfaz: PCIe 4.0 x4 NVMe
Lectura secuencial: 5150 MB/s
Escritura secuencial: 4850 MB/s
Durabilidad: 1200 TBW'
	where Codigo = 'sdd8';
update Articulos set Marca = 'Adata', GarantiaMeses = 36, DescripcionLarga = 'Disco sólido Adata de 1 TB. Reduce los tiempos de arranque y de carga de forma notable frente a un disco rígido.',
	Especificaciones = 'Capacidad: 1 TB
Formato: M.2 2280
Interfaz: PCIe 4.0 x4 NVMe
Lectura secuencial: 5000 MB/s
Escritura secuencial: 4500 MB/s
Durabilidad: 740 TBW'
	where Codigo = 'sdd9';
