using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OnXap.Core.Data
{
    /// <summary>
    /// </summary>
    public static class ValidationExceptionExtensions
    {
        /// <summary>
        /// Возвращает комплексное сообщение об ошибке.
        /// </summary>
        /// <param name="exception"></param>
        /// <param name="glueBefore">Подставляется перед каждой ошибкой.</param>
        /// <param name="glueAfter">Подставляется после каждой ошибки.</param>
        /// <returns></returns>
        public static string CreateComplexMessage(this ValidationException exception, string glueBefore = " - ", string glueAfter = ";\r\n")
        {
            var error = "";
            var parts = new List<string>();
            throw new InvalidProgramException();
            //todo разобраться, как должно работать.
            //foreach (var _error in exception..EntityValidationErrors)
            //{
            //    if (!_error.IsValid)
            //    {
            //        foreach (var __error in _error.ValidationErrors)
            //        {
            //            var errorMessage = glueBefore + __error.ErrorMessage;
            //            if (!string.IsNullOrEmpty(glueAfter))
            //            {
            //                if (!errorMessage.Last().In('.', ',', ';', '!', '?')) errorMessage += glueAfter;
            //            }

            //            parts.Add(errorMessage);
            //        }
            //    }
            //}

            error = string.Join("", parts);


            return error;
        }
    }
}
