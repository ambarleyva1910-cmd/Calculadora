using System.Globalization;

namespace Calculadora
{
    public partial class MainPage : ContentPage
    {
        string operacion = "";
        bool resultadoMostrado = false;

        private Label lblOperacionRef;
        private Label lblResultadoRef;

        public MainPage()
        {
            InitializeComponent();

            
            lblOperacionRef = this.FindByName<Label>("lblOperacion");
            lblResultadoRef = this.FindByName<Label>("lblResultado");
        }

        private void OnButtonClicked(object sender, EventArgs e)
        {
            Button boton = (Button)sender;
            string valor = boton.Text;

            
            if (valor == "AC")
            {
                operacion = "";
                resultadoMostrado = false;

                lblOperacionRef.Text = "";
                lblResultadoRef.Text = "0";
                return;
            }

            
            if (valor == "DEL")
            {
                if (resultadoMostrado)
                {
                    operacion = "";
                    resultadoMostrado = false;
                }
                else if (operacion.Length > 0)
                {
                    operacion = operacion.Substring(
                        0, operacion.Length - 1);
                }

                ActualizarPantalla();
                return;
            }

           
            if (valor == "=")
            {
                if (operacion == "")
                    return;

                try
                {
                    string expresion = operacion;

                    double resultado =
                        new CalculadoraMatematica(expresion).Calcular();

                    lblOperacionRef.Text = expresion + " =";

                    operacion = resultado.ToString(
                        "G15", CultureInfo.InvariantCulture);

                    lblResultadoRef.Text = operacion;
                    resultadoMostrado = true;
                }
                catch
                {
                    lblOperacionRef.Text = "";
                    lblResultadoRef.Text = "Error";
                    operacion = "";
                    resultadoMostrado = true;
                }

                return;
            }

           
            if (resultadoMostrado)
            {
                if (EsOperador(valor) || valor == "%")
                {
                    resultadoMostrado = false;
                }
                else
                {
                    operacion = "";
                    resultadoMostrado = false;
                    lblOperacionRef.Text = "";
                }
            }

            
            if (valor == ".")
            {
                if (operacion.Length == 0 ||
                    EsOperador(operacion[^1].ToString()) ||
                    operacion[^1] == '%')
                {
                    operacion += "0.";
                }
                else
                {
                    string numero = ObtenerUltimoNumero();

                    if (!numero.Contains("."))
                        operacion += ".";
                }

                ActualizarPantalla();
                return;
            }

             
            if (valor == "%")
            {
                if (operacion.Length > 0 &&
                    (char.IsDigit(operacion[^1]) ||
                     operacion[^1] == ')'))
                {
                    operacion += "%";
                }

                ActualizarPantalla();
                return;
            }

            
            if (EsOperador(valor))
            {
                if (operacion.Length == 0)
                {
                    if (valor == "-")
                        operacion = "-";

                    ActualizarPantalla();
                    return;
                }

                char ultimo = operacion[^1];

                if (EsOperador(ultimo.ToString()))
                {
                   
                    if (valor == "-" &&
                        ultimo != '-')
                    {
                        operacion += "-";
                    }
                    else if (operacion.Length > 1 &&
                             EsOperador(
                                 operacion[^2].ToString()))
                    {
                        operacion = operacion.Substring(
                            0, operacion.Length - 2) + valor;
                    }
                    else
                    {
                        operacion = operacion.Substring(
                            0, operacion.Length - 1) + valor;
                    }
                }
                else if (ultimo != '.')
                {
                    operacion += valor;
                }

                ActualizarPantalla();
                return;
            }

            
            if (valor.Length == 1 &&
                char.IsDigit(valor[0]))
            {
                operacion += valor;
                ActualizarPantalla();
            }
        }

        private bool EsOperador(string valor)
        {
            return valor == "+" ||
                   valor == "-" ||
                   valor == "×" ||
                   valor == "÷";
        }

        private string ObtenerUltimoNumero()
        {
            int posicion = operacion.Length - 1;

            while (posicion >= 0 &&
                   (char.IsDigit(operacion[posicion]) ||
                    operacion[posicion] == '.'))
            {
                posicion--;
            }

            return operacion.Substring(posicion + 1);
        }

        private void ActualizarPantalla()
        {
            lblResultadoRef.Text =
                operacion == "" || operacion == "-"
                    ? operacion == "" ? "0" : "-"
                    : operacion;
        }
    }

    
    public class CalculadoraMatematica
    {
        private readonly string texto;
        private int posicion;

        public CalculadoraMatematica(string expresion)
        {
            texto = expresion
                .Replace("×", "*")
                .Replace("÷", "/");

            posicion = 0;
        }

        public double Calcular()
        {
            double resultado = Expresion();

            if (posicion != texto.Length)
                throw new Exception("Expresión inválida");

            if (double.IsNaN(resultado) ||
                double.IsInfinity(resultado))
            {
                throw new Exception("Resultado inválido");
            }

            return resultado;
        }

        
        private double Expresion()
        {
            double resultado = Termino();

            while (posicion < texto.Length)
            {
                char operador = texto[posicion];

                if (operador == '+' || operador == '-')
                {
                    posicion++;
                    double numero = Termino();

                    if (operador == '+')
                        resultado += numero;
                    else
                        resultado -= numero;
                }
                else
                {
                    break;
                }
            }

            return resultado;
        }

        
        private double Termino()
        {
            double resultado = Factor();

            while (posicion < texto.Length)
            {
                char operador = texto[posicion];

                if (operador == '*' || operador == '/')
                {
                    posicion++;
                    double numero = Factor();

                    if (operador == '*')
                    {
                        resultado *= numero;
                    }
                    else
                    {
                        if (numero == 0)
                            throw new Exception(
                                "No se puede dividir entre cero");

                        resultado /= numero;
                    }
                }
                else
                {
                    break;
                }
            }

            return resultado;
        }

        
        private double Factor()
        {
            if (posicion >= texto.Length)
                throw new Exception("Falta un número");

            
            if (texto[posicion] == '-')
            {
                posicion++;
                return -Factor();
            }

            
            if (texto[posicion] == '+')
            {
                posicion++;
                return Factor();
            }

            int inicio = posicion;
            bool tienePunto = false;
            bool tieneDigito = false;

            while (posicion < texto.Length)
            {
                char caracter = texto[posicion];

                if (char.IsDigit(caracter))
                {
                    tieneDigito = true;
                    posicion++;
                }
                else if (caracter == '.')
                {
                    if (tienePunto)
                        throw new Exception(
                            "Número decimal inválido");

                    tienePunto = true;
                    posicion++;
                }
                else
                {
                    break;
                }
            }

            if (!tieneDigito)
                throw new Exception("Número inválido");

            string numeroTexto = texto.Substring(
                inicio, posicion - inicio);

            double numero = double.Parse(
                numeroTexto, CultureInfo.InvariantCulture);

            
            while (posicion < texto.Length &&
                   texto[posicion] == '%')
            {
                numero /= 100;
                posicion++;
            }

            return numero;
        }
        
    }
}
