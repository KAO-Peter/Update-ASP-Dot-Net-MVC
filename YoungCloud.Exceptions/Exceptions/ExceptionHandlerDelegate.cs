namespace YoungCloud.Exceptions
{
    /// <summary>
    /// To handle exception of framework.
    /// </summary>
    /// <param name="sender">The object instance that invoke this handle process.</param>
    /// <param name="e">The instance of <see cref="ExceptionHandlerEventArgs">ExceptionHandlerEventArgs</see>.</param>
    public delegate void ExceptionHandlerDelegate(object sender, ExceptionHandlerEventArgs e);
}