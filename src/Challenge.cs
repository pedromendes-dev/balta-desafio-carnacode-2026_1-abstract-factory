#:property PublishAot=false

// DESAFIO: Sistema de Pagamentos Multi-Gateway
// PROBLEMA: Uma plataforma de e-commerce precisa integrar com múltiplos gateways de pagamento
// (PagSeguro, MercadoPago, Stripe) e cada gateway tem componentes específicos (Processador, Validador, Logger)
// O código atual está muito acoplado e dificulta a adição de novos gateways

using System;
using System.Collections.Generic;

namespace DesignPatternChallenge
{
    public interface IPaymentProcessor
    {
        string ProcessTransaction(decimal amount, string cardNumber);
    }

    public interface IPaymentValidator
    {
        bool ValidateCard(string cardNumber);
    }

    public interface ITransactionLogger
    {
        void Log(string message);
    }

    public interface IPaymentGatewayFactory
    {
        IPaymentValidator CreateValidator();
        IPaymentProcessor CreateProcessor();
        ITransactionLogger CreateLogger();
    }

    public class PaymentService
    {
        private readonly IPaymentValidator _validator;
        private readonly IPaymentProcessor _processor;
        private readonly ITransactionLogger _logger;

        public PaymentService(IPaymentGatewayFactory factory)
        {
            _validator = factory.CreateValidator();
            _processor = factory.CreateProcessor();
            _logger = factory.CreateLogger();
        }

        public void ProcessPayment(decimal amount, string cardNumber)
        {
            if (!_validator.ValidateCard(cardNumber))
            {
                _logger.Log("Cartão inválido");
                return;
            }

            var transactionId = _processor.ProcessTransaction(amount, cardNumber);
            _logger.Log($"Transação processada: {transactionId}");
        }
    }

    // Família PagSeguro
    public class PagSeguroValidator : IPaymentValidator
    {
        public bool ValidateCard(string cardNumber)
        {
            Console.WriteLine("PagSeguro: Validando cartão...");
            return cardNumber.Length == 16;
        }
    }

    public class PagSeguroProcessor : IPaymentProcessor
    {
        public string ProcessTransaction(decimal amount, string cardNumber)
        {
            Console.WriteLine($"PagSeguro: Processando R$ {amount}...");
            return $"PAGSEG-{Guid.NewGuid().ToString()[..8]}";
        }
    }

    public class PagSeguroLogger : ITransactionLogger
    {
        public void Log(string message)
        {
            Console.WriteLine($"[PagSeguro Log] {DateTime.Now}: {message}");
        }
    }

    public class PagSeguroFactory : IPaymentGatewayFactory
    {
        public IPaymentValidator CreateValidator() => new PagSeguroValidator();
        public IPaymentProcessor CreateProcessor() => new PagSeguroProcessor();
        public ITransactionLogger CreateLogger() => new PagSeguroLogger();
    }

    // Família MercadoPago
    public class MercadoPagoValidator : IPaymentValidator
    {
        public bool ValidateCard(string cardNumber)
        {
            Console.WriteLine("MercadoPago: Validando cartão...");
            return cardNumber.Length == 16 && cardNumber.StartsWith("5");
        }
    }

    public class MercadoPagoProcessor : IPaymentProcessor
    {
        public string ProcessTransaction(decimal amount, string cardNumber)
        {
            Console.WriteLine($"MercadoPago: Processando R$ {amount}...");
            return $"MP-{Guid.NewGuid().ToString()[..8]}";
        }
    }

    public class MercadoPagoLogger : ITransactionLogger
    {
        public void Log(string message)
        {
            Console.WriteLine($"[MercadoPago Log] {DateTime.Now}: {message}");
        }
    }

    public class MercadoPagoFactory : IPaymentGatewayFactory
    {
        public IPaymentValidator CreateValidator() => new MercadoPagoValidator();
        public IPaymentProcessor CreateProcessor() => new MercadoPagoProcessor();
        public ITransactionLogger CreateLogger() => new MercadoPagoLogger();
    }

    // Família Stripe
    public class StripeValidator : IPaymentValidator
    {
        public bool ValidateCard(string cardNumber)
        {
            Console.WriteLine("Stripe: Validando cartão...");
            return cardNumber.Length == 16 && cardNumber.StartsWith("4");
        }
    }

    public class StripeProcessor : IPaymentProcessor
    {
        public string ProcessTransaction(decimal amount, string cardNumber)
        {
            Console.WriteLine($"Stripe: Processando ${amount}...");
            return $"STRIPE-{Guid.NewGuid().ToString()[..8]}";
        }
    }

    public class StripeLogger : ITransactionLogger
    {
        public void Log(string message)
        {
            Console.WriteLine($"[Stripe Log] {DateTime.Now}: {message}");
        }
    }

    public class StripeFactory : IPaymentGatewayFactory
    {
        public IPaymentValidator CreateValidator() => new StripeValidator();
        public IPaymentProcessor CreateProcessor() => new StripeProcessor();
        public ITransactionLogger CreateLogger() => new StripeLogger();
    }

    public static class PaymentGatewayFactoryProvider
    {
        private static readonly Dictionary<string, IPaymentGatewayFactory> Factories = new(StringComparer.OrdinalIgnoreCase)
        {
            ["pagseguro"] = new PagSeguroFactory(),
            ["mercadopago"] = new MercadoPagoFactory(),
            ["stripe"] = new StripeFactory()
        };

        public static IPaymentGatewayFactory GetFactory(string gateway)
        {
            if (Factories.TryGetValue(gateway, out var factory))
                return factory;

            throw new ArgumentException("Gateway não suportado");
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== Sistema de Pagamentos ===\n");

            var pagSeguroFactory = PaymentGatewayFactoryProvider.GetFactory("pagseguro");
            var pagSeguroService = new PaymentService(pagSeguroFactory);
            pagSeguroService.ProcessPayment(150.00m, "1234567890123456");

            Console.WriteLine();

            var mercadoPagoFactory = PaymentGatewayFactoryProvider.GetFactory("mercadopago");
            var mercadoPagoService = new PaymentService(mercadoPagoFactory);
            mercadoPagoService.ProcessPayment(200.00m, "5234567890123456");

            Console.WriteLine();

            var stripeFactory = PaymentGatewayFactoryProvider.GetFactory("stripe");
            var stripeService = new PaymentService(stripeFactory);
            stripeService.ProcessPayment(300.00m, "4234567890123456");

            Console.WriteLine();
        }
    }
}
