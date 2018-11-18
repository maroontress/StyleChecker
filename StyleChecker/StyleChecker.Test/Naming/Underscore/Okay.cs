namespace Application
{
    public sealed class Okay
    {
        public void DiscardDesignation()
        {
            _ = "hello".Length;

            (int, int) NewPoint(int x, int y) => (x, y);
            var (one, _) = NewPoint(1, 2);

            void Out(out int x) => x = 3;
            Out(out _);
        }
    }
}
