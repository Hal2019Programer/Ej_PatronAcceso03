using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using System.Diagnostics;
using Xamarin.Essentials;

namespace Ej_PatronAcceso03
{
    public partial class MainPage : ContentPage
    {
        private List<int> selectedPoints = new List<int>();
        private readonly List<SKPoint> points = new List<SKPoint>();
        int numero_PaintSurface;

        public MainPage()
        {
            InitializeComponent();
            InitializePoints();
        }

        // ConvertToPixels convierte un valor en puntos de una propiedad de un objeto
        // de Xamarin.Forms en un valor en pixeles usando para ello información del
        // dispositivo donde se ejecuta.
        // La información obtenida a de DeviceDisplay.MainDisplayInfo permite
        // usar la propiedad Density para obtener el factor de densidad del dispositivo
        // y con ella convertir a su equivalente en pixeles
        public float ConvertToPixels(float valueInPoints)
        {
            // Obtener la información de la pantalla
            var displayInfo = DeviceDisplay.MainDisplayInfo;

            // Obtener la densidad de píxeles (DPI)
            float density = (float)displayInfo.Density; // Esto es el factor de densidad
            //Debug.WriteLine($"Densidad del disposiivo: {density}");
            // Convertir puntos a píxeles
            float valueInPixels = valueInPoints * density;

            return valueInPixels;
        }
        // InitializePoints inicializa el conjunto de puntos (9 en total) que se dibujará
        // posteriormente en el objeto SKCanvasView cuyo nombre asignado es 'CanvasView'.
        // Para ello almacena cada punto en una lista List<SKPoint> points = new List<SKPoint>()
        // de tipo SKPoint que contiene dos valores X e Y para las coordenadas donde se
        // deben colocar los puntos. Esta lista es publica dentro de la clase MainPage
        private void InitializePoints()
        {
            // Obtener densidad del dispositivo y ajustar valores para los pixeles
            float heightRequest = (float)CanvasView.HeightRequest; // Por ejemplo, 100 puntos
            float heightInPixels = ConvertToPixels(heightRequest);
            Debug.WriteLine($"Valor del CanvasView.HeightRequest: {heightInPixels}");
            //--------------------------------------------------------------------
            float spacing = 200; // Espaciado entre puntos
            //float offset = 100;  // Margen desde la izquierda y arriba
            // Se reajusta la posicion del offset para centrarlo en el SKCanvasView según 
            // la densidad del dispositivo, tomndo en cuenta el HeightRequest y WidthRequest
            // del área de dibujo representado por PaintSurface
            float offset = (heightInPixels-(520-100))/2;
            // 520 es la posición del limite maximo del ultimo punto que se dibuja en el
            // PaintSurface y 100 es el inicio de las coordenadas X e Y desde donde se dibuja
            // todos los puntos. Al restarlo se obtiene el ancho y alto maximo donde deben
            // estar mostrarse los puntos dentro del PaintSurface, de modo que al usarlo
            // reajusta el origen real del offset dentro del área de dibujo en el centro

            //Debug.WriteLine("--------------InitializePoints----------------");
            for (int row = 0; row < 3; row++)
            {
                for (int col = 0; col < 3; col++)
                {
                    points.Add(new SKPoint(offset + col * spacing, offset + row * spacing));
                    Debug.WriteLine($"x={offset + col * spacing},y={offset + row * spacing}");
                }
            }
            //Debug.WriteLine("---------------------------------------------");
        }

        // OnCanvasTouch es el método que se activa cada vez que se presiona o se desliza el dedo
        // por la pantalla del dispositivo móvil.
        // En el código maneja dos situaciones:
        // a. Cuando se ha presionado la pantalla (SKTouchAction.Pressed) o cuando se está deslizando
        //    el dedo por la pantalla (SKTouchAction.Moved):
        //
        //    if (e.ActionType == SKTouchAction.Pressed || e.ActionType == SKTouchAction.Moved)
        //
        //    En este caso guarda los puntos (0 al 8) por donde se ha presionado o deslizado el dedo
        //    en la lista 'selectedPoints' y llama al método InvalidateSurface de SKCanvasView
        //    para que redibuje el lienzo de PaintSurface.
        // b. En caso de dejar de presionar o deslizar el dedo en el pantalla del dispositivo móvil
        //    (SKTouchAction.Released) se ejecuta una comprobación de los puntos presionados
        //    o deslizados con ValidatePattern(), luego limpia la lista de puntos con
        //    selectedPoints.Clear() y vuelve a dibujar el lienzo con el método InvalidateSurface.
        private void OnCanvasTouch(object sender, SKTouchEventArgs e)
        {
            Debug.WriteLine("**********  Ingreseo a OnCanvasTouch");
            if (e.ActionType == SKTouchAction.Pressed || e.ActionType == SKTouchAction.Moved)
            {
                Debug.WriteLine("**********  Ingreseo a Pressed o Moved");
                for (int i = 0; i < points.Count; i++)
                {
                    if (VectorDistance(points[i], new SKPoint(e.Location.X, e.Location.Y)) < 50 && !selectedPoints.Contains(i))
                    {
                        selectedPoints.Add(i);
                        Debug.WriteLine($"x={e.Location.X},y={e.Location.Y}");
                        break;
                    }
                }
                CanvasView.InvalidateSurface(); // Redibujar
            }
            else if (e.ActionType == SKTouchAction.Released)
            {
                ValidatePattern();
                selectedPoints.Clear();
                CanvasView.InvalidateSurface();
            }
            e.Handled = true;
        }

        // OnCanvasViewPaintSurface es un evento que se activa cada vez que se dibuja en el 
        // SKCanvasView cuyo nombre asignado es 'CanvasView'.
        // El atributo configurado es PaintSurface="OnCanvasViewPaintSurface" y la primera vez
        // se activa al mostrarse el objeto SKCanvasView. Posteriormente se llama al realizar
        // cualquier acción o gesto del Touch, que también se ha asignado al SKCanvasView con el
        // atributo Touch="OnCanvasTouch", donde 'OnCanvasTouch' es el método que cnstantemente
        // se esta activando con el Touch.
        // En el código, lo primero se hace es asignar a SKCanvasView el color de fondo White:
        // canvas.Clear(SKColors.White);
        // Luego se define un color Orange para las líneas a dibujar en el patron con:
        // var paint = new SKPaint
        // Posteriormente, si hay valores en la variable de lista 'selectedPoints' (que almacena
        // solo los números enteros correspondientes a los 9 puntos, de 0 a 8) se dibuja la línea
        // de color naranja de punto a punto marcado en el deslizamiento del Touch. Si no hay 
        // valores se salta el for (int i = 0; i < selectedPoints.Count - 1; i++)
        // Luego se define el color 'Blue' y relleno completo de los puntos (los 9 puntos a dibujar)
        // var dotPaint = new SKPaint
        // y finalmente se dibuja cada vez que se llama a 'OnCanvasViewPaintSurface':
        // foreach (var point in points)
        private void OnCanvasViewPaintSurface(object sender, SKPaintSurfaceEventArgs args)
        {
            numero_PaintSurface++;
            Debug.WriteLine($"Dibujo con OnCanvasViewPaintSurface numero {numero_PaintSurface}");
            var canvas = args.Surface.Canvas;
            canvas.Clear(SKColors.White);

            var paint = new SKPaint
            {
                Color = SKColors.Orange,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 5
            };

            // Dibujar líneas del patrón
            for (int i = 0; i < selectedPoints.Count - 1; i++)
            {
                canvas.DrawLine(points[selectedPoints[i]], points[selectedPoints[i + 1]], paint);
            }

            // Dibujar los puntos
            var dotPaint = new SKPaint
            {
                Color = SKColors.Blue,
                Style = SKPaintStyle.Fill
            };

            foreach (var point in points)
            {
                canvas.DrawCircle(point, 20, dotPaint);
            }
        }
        // ValidatePattern() verifica que los puntos de la variable de lista selectedPoints
        // coinciden con el conjunto de puntos establecidos en la variable 'correctPattern'.
        // Para ello compara la secuencia de ambas variables con:
        //
        // selectedPoints.SequenceEqual(correctPattern)
        //
        // Y si coinciden, se muestra un mensaje de patrón correcto o incorrecto.
        private void ValidatePattern()
        {
            //int lista_correcta = 0;
            var correctPattern = new List<int> { 0, 1, 2, 5, 8 }; // Patrón de desbloqueo
            //var lista_numeros = new List<List<int>>();
            //lista_numeros.Add(new List<int> { 0, 1, 2, 5, 8 }); // Primera lista
            //lista_numeros.Add(new List<int> { 0, 3, 6 });    // Segunda lista
            //lista_numeros.Add(new List<int> { 0, 1, 4, 7, 8 });
            //lista_numeros.Add(new List<int> { 3, 0, 1, 2, 4, 6, 7, 8, 5 });
            //lista_numeros.Add(new List<int> { 0, 4, 8, 7, 6, 2 });
            //lista_numeros.Add(new List<int> { 2, 5, 8, 7, 6, 3, 0, 1 });
            //for (int i = 0; i < lista_numeros.Count; i++)
            //{
            //    if (selectedPoints.SequenceEqual(lista_numeros[i]))
            //    {
            //        lista_correcta = i+1;
            //    }
            //}
            //if (lista_correcta>0)
            //{
            //    DisplayAlert("Desbloqueado", "Patrón correcto", "OK");
            //}
            //else
            //{
            //    DisplayAlert("Error", "Patrón incorrecto", "Reintentar");
            //}

            if (selectedPoints.SequenceEqual(correctPattern))
            {
                DisplayAlert("Desbloqueado", "Patrón correcto", "OK");
            }
            else
            {
                DisplayAlert("Error", "Patrón incorrecto", "Reintentar");
            }
        }

        private float VectorDistance(SKPoint p1, SKPoint p2)
        {
            return (float)Math.Sqrt(Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2));
        }

    }
}
