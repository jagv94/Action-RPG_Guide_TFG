# Demo Técnica: Guía para el Desarrollo de Interacciones en VR (UI/UX en Action-RPG)

Este repositorio contiene el código fuente de una demo técnica de Realidad Virtual (VR) desarrollada como parte del Trabajo de Fin de Grado (TFG) titulado **"Guía para el Desarrollo de Interacciones en VR"**. El proyecto se enfoca específicamente en abordar los desafíos de la Experiencia de Usuario (UX) en videojuegos de VR del género Action-RPG, como el *motion sickness*, la fatiga visual y la sobrecarga cognitiva.

La demo fue creada para investigar y proponer **soluciones prácticas y replicables para optimizar las interfaces y las interacciones** en este tipo de entornos inmersivos [2-4]. Su desarrollo siguió una metodología basada en la investigación bibliográfica, el diseño de una guía de buenas prácticas y la implementación empírica.

**Autor:** Jose Adrián Guerra Viudez
**Tutor:** José Luis Eguia Gómez
**Trabajo de Fin de Grado:** Guía para el Desarrollo de Interacciones en VR
**Grado:** Grau en Continguts Digitals Interactius
**Universidad:** ENTI-UB, Universitat de Barcelona
**Año Académico:** 2024-2025 (Basado en contexto del TFG)

## Objetivos de la Demo Técnica

El objetivo principal de esta demo técnica fue **implementar las recomendaciones de diseño** establecidas en la guía y **validar su aplicabilidad práctica** mediante pruebas con usuarios y desarrolladores [3, 5-7]. Sirvió como prototipo funcional para evaluar la efectividad de las soluciones propuestas en un entorno interactivo.

## Características Implementadas (Versión de Playtesting)

Esta versión de la demo técnica implementa diversas soluciones de UX para VR, centradas en el confort y la usabilidad en un contexto Action-RPG:

*   **Sistemas de Locomoción Personalizables:** Incluye opciones de movimiento continuo (con ajustes como el efecto *tunneling vignette*) y teletransporte instantáneo. La rotación por tramos (*snap rotation*) también está configurada.
*   **Interfaces Interactivas:** Menús flotantes y adaptativos (como el menú de pausa) diseñados para ser accesibles y reducir la sobrecarga cognitiva. Se basaron en bocetos previos para su organización. Incluye un menú principal, menú de partida (nueva/cargar), menú de ajustes (locomoción, gráficos, etc.), y menús de pausa (inventario, habilidades, encargos/misiones). Los menús asociados a las mecánicas de juego se incluyen como plantilla a adaptar/terminar para un juego/experiencia completo.
*   **Ajustes Visuales y de Optimización:** Opciones de configuración para modificar parámetros como la distancia de renderizado de sombras, Screen Space Reflections, y Dynamic Resolution Scaling (DRS) para PCVR. Nota: Los ajustes para los reflejos no están correctamente implementados.
*   **Entorno de Prueba:** Incluye áreas para exploración, un breve viaje no interactivo en barca y un puzzle básico para probar interacciones (los orbes se activan mediante el contacto con el trigger del cuerpo del jugador, si no funciona, suele servir dar una vuelta al rededor del pilar en cuestión, ya que el collider se desconfigura y aleja un poco del cuerpo principal del jugador).
*   **Sistema Básico de Datos:** Un sistema para recopilar datos anónimos de interacción (duración de sesión, acceso a ajustes, uso de teletransporte, etc.) y rendimiento (FPS) durante las pruebas, con opción de consentimiento revocable por el usuario.

## Tecnologías Utilizadas

*   **Motor Gráfico:** Unity
*   **Frameworks VR:** Unity XR Interaction Toolkit, Oculus Integration SDK
*   **Visor de Desarrollo/Prueba Principal:** Meta Quest 3 en modo PCVR
*   **Backend de Datos (Opcional):** Firebase (utilizado para la recopilación de datos anónimos en los tests)

## Versión Utilizada para Validación (Playtest Version)

El código en este repositorio, bajo el tag **"Playtest version"**, corresponde a la versión específica de la demo técnica que fue utilizada para la fase de validación empírica del TFG. Esta versión fue sometida a pruebas con usuarios y desarrolladores, evaluando el confort mediante el **Simulator Sickness Questionnaire (SSQ)** y la usabilidad mediante el **System Usability Scale (SUS)**.

Los resultados de estas pruebas, incluyendo el análisis del SSQ, SUS y feedback cualitativo, se detallan en la memoria del TFG, sirviendo para validar la efectividad de las soluciones implementadas y señalar áreas de mejora. No se implementaron iteraciones posteriores sobre esta versión específica de la demo.

## Cómo Ejecutar la Demo

1.  Clonar el repositorio.
2.  Abrir el proyecto en Unity (Unity 6 (6000.0.32F1)).
3.  Conectar un visor Meta Quest en modo PCVR (por ejemplo, con Meta Quest Link).
4.  Abrir la escena principal (`Assets/_MainProject/Scenes/MainMenu.unity`).
5.  Presionar Play en el editor de Unity o compilar y ejecutar el proyecto.

Nota: Con el fin de la realización del proyecto, la base de datos a la que apunta está desactivada y es probable que de errores por ello. Se recomienda desactivar el sistema de captura de datos permanentemente (desactivar el game object TrackingManager en la escena de menú (MainMenu) y la escena de juego (GameScene)) o añadir una base de datos activa para la captura (en el script FirebaseManager.cs).

## Enlace a la Memoria del TFG

Para consultar la investigación completa, el diseño de la guía, la metodología detallada, los resultados de la validación y las conclusiones del proyecto, puedes acceder a la memoria del TFG completa una vez que esté publicada en el Dipòsit Digital de la Universitat de Barcelona.

[Enlace al Dipòsit Digital de la UB - Placeholder]

## Licencia

El código fuente de esta demo técnica se comparte bajo la licencia **Creative Commons Reconocimiento-NoComercial-SinObrasDerivadas (BY-NC-ND 4.0 Internacional)**.

Esto significa que eres libre de:
*   **Compartir** — copiar y redistribuir el material en cualquier medio o formato.

Bajo las siguientes condiciones:
*   **Reconocimiento** — Debes dar el crédito adecuado, proporcionar un enlace a la licencia e indicar si se han realizado cambios.
*   **No Comercial** — No puedes utilizar el material con fines comerciales.
*   **Sin Obras Derivadas** — Si remezclas, transformas o construyes sobre el material, no puedes distribuir el material modificado.

La titularidad de los derechos morales y de explotación sobre la obra pertenece y seguirá perteneciendo al autor.

## Líneas de Mejora Futuras

Basado en los resultados del testeo y la investigación, se identificaron diversas áreas para futuras iteraciones y mejoras del proyecto:

*   Implementación de otras variantes de locomoción (teletransporte con fundido a negro, teletransporte con trayecto visible).
*   Optimización técnica (estabilidad de FPS, tiempos de carga reducidos, eliminación de artefactos visuales).
*   Rediseño y simplificación de interfaces.
*   Incorporación de un sistema de onboarding interactivo.
*   Expansión del contenido para incluir mecánicas clave de Action-RPG (combate, progresión, inventario complejo, misiones).
*   Exploración de interacciones naturales con seguimiento de manos (*hand tracking*).
*   Investigación sobre personalización de confort adaptada a perfiles de usuario específicos.
*   Adaptación del modelo a entornos de Realidad Mixta (MR).
*   Aplicación de la metodología a otros géneros o sectores de VR.