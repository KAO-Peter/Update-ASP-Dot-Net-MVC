using AutoMapper;

namespace YoungCloud.Configurations.AutoMapperSetting
{
    public partial class CommonMapperConfiguration : Profile
    {
        protected override void Configure()
        {
            base.Configure();
        }

        public override string ProfileName
        {
            get { return this.GetType().Name; }
        }
    }
}
