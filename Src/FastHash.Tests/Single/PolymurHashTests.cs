// ReSharper disable StringLiteralTypo
// ReSharper disable IdentifierTypo
using System.Text;
using Genbox.FastHash.PolymurHash;

namespace Genbox.FastHash.Tests.Single;

public class PolymurHashTests
{
    private string[] POLYMUR_TEST_STRINGS =
    {
        "",
        "i",
        "es",
        "vca",
        "bdxa",
        "bbbmc",
        "vn5719",
        "lpvif62",
        "1fcjgark",
        "1jlz2nr6w",
        "g4q6ebxvod",
        "ehiybujo2n1",
        "6u2990ulzi7m",
        "c3xcb4ew8v678",
        "bhcaqrm221pea1",
        "oyl3iqxqr85eeve",
        "b41kacwmnim8rup5",
        "563ug64z3zdtlj438",
        "3spvl57qfg4udw2l3s",
        "297r1bqesqdhb3jd50g",
        "kbc5btot9x1fqslddmha",
        "r0vxw6kk8tc6pk0oxnr6m",
        "wkgmmma9icgky3bnj5bjir",
        "5eslfmq1w3i7wvd89ls7nvf",
        "40ytv0ye8cq49no6ys1pdrot",
        "p3mbto6bl36g3cx9sstyiugsd",
        "m0ylpn0wh5krbebs0j5trzgveb",
        "qsy8gpheo76vb8g0ivaojk1zgk4",
        "dwqf8tpad4k3x69sah7pstrg8zxx",
        "ls3zrsjf1o3cr5sjy7dzp98198i3y",
        "xvhvx3wbzer9b7kr4jqg2ok9e3mv5d",
        "yapzlwab361wvh0xf1rydn5ynqx8cz0",
        "nj56v1p9dc7qdmcn2wksfg5kic1uegm2",
        "hlebeoafjqtqxfwd9ge94z3ofk88c4a5x",
        "6li8qyu0n8nwoggm4hqzqdamem5barzjyw",
        "wj7sp7dhpfapsd8w2nzn8s7xtnro9g45x7t",
        "ahio6so1x30oziw54ux5iojjdfvkwpw2v14d",
        "wm6yacnl6k3kj3c6i1jeajuwmquv9yujms0wq",
        "kzs6xfhmc4ifmstnekcze4y1l83ddvxust2r0o",
        "ckamexupx7cmsuza9nssw6n45e7go4s3osr1903",
        "nob5bj9tok346dg62jbfjfrhg5l6itsno2hkhfru",
        "vgo0ko42n5jvrvnv3ddpwg8h7gkqoxbllv2fdy0no",
        "dgs47djqzq3czo0i0v1u3d3x72vtvi3w2tsf9shx6k",
        "8vjrw7jz90kf969txb5qrh0u5332zf5epsp8aes4aqh",
        "3ni9vtqiq6vnxipfa2wag8vfwq2nyce1kgq5nj3razx9",
        "u29xjkod6rtu5j5tlwkydt9khih6o2do84q6ukwlr00xf",
        "yxxubvyxuusw827qctqr6tmm69rij5ex2zk1etps8qh61e",
        "p7lh4mvadnp6uw0vt7bnzcbv1wjswuuc6gjmu684yznx8lp",
        "8c27lotvnab6ra8pq9aon0w30ydyulesinew3akqrhhmm39e",
        "ttipbm97gpk7tiog1doncalwgpb7alk16dapga2ekzjt59pv6",
        "mbbtplseab2mgtgh8uwlhbmdrwxae3tc2mtf98bwuhmz4bfjnf",
        "shnjeydnj8awrkz3rd69wqqd9srie4eo6gc6ylhz2ouv4t4qbar",
        "lckl12agnpr6q5053h9v38lyk71emkvwdzrv0ic3a4a4pn3w3o4x",
        "7927wqjo5jiecfk0bbtt6065j5jl7x0vv1mcxxxl0j1oatrom44zp",
        "bajk3ff026vx0u7o5d7ry7w7n07sqdy4urv4psr79jp13e0mxsks1r",
        "en6j5o90gmgj7ssbz6jv3kzdsbzczu518c3zmezkp02rtvo1s88n9pu",
        "58fkwyf44tjnrytgplb5qfbvlwtav3zutxowoor2mklkr2up4nzpefos",
        "cep02qfl6swv1j3mwy5kprm4p8drszchufrkyr5ejbtzgu5cti6fqab5c",
        "lr5q0p1dljga8h4vruy1doa79hntwbdyolnh1fbe3phfk7f5rgs4815foj",
        "hmnjq6h1sslivjzmbxbpqba29f6kvbea6n6c4sanm40nzmrxt8hm61ooq3e",
        "ae43xxu1mqrbynmctit7m4wf02o0kf2vvw1l3y51n4cu5v5ba4dia67wf0bo",
        "qz9ye2ur849obmm23d5tnfc3xdaeajil0gm2pz8z9psedj50h5hcwbcn8n2lo",
        "w3xar1pzaff7fhyw6cshdgechm2pj1ebwrbkdct5xfbmxskr3937dodvky62i8",
        "ypy5k197quc9ypqoj9kle2eky307jnnd7tu52hqhn6mo7jj1fvmi42kkgq40iy6",
        "k1bp6qwiul8fnd6rfe42ge6gskk0jkr9fjgmuujey3kn8ie88h9qguw2gboo7i80",
        "begb64jkzfujx7ch3ain1iixidnbhcbcglcuf7nys8eansnkewtiye9xv7s2ksuev",
        "vf5d8vdjtwp5vo1ocb274nkl6h8vg97m4v5htfwv02tj9u68vdnteeim6q0zllxflj",
        "dcg9osulcdw9sqaue4cfz6k990vpstoxmvwbxzhzichkhdujy36v556u7oxug51gdup",
        "1rtgdtibcaos4ebzrbl1fkjahtbel6fyqipuu8lxfrwnggjr8wgoscfxp46wv9wjk315",
        "r27qj342zj4anpkqpr9yqo7udnldwiqqpq667zzjgw33yia3wt2p6t221onq4pvfaywbj",
        "2yzxskad06pt9zvjmiobfz12a3q6wqgpj4450rpxj0jvjk3cx39qo6cbpukxqsy6idqd40",
        "813zultj26k3gn6gibolpuozgaxu8exfatf4iqqugelcf6k8dnzvsjb9s25g3gyess2uscc",
        "i4p0jkxf3ajc02x330y3tg8l521fzootabn53ovru20ph3n17hfygaz1axs61jxipz6jac5z",
        "5bk748kkvww7toeyeueukk2qyin2o5ohnvj7l1cqs9zgy92n6ujxg6sxdjw81hfd29nzrb4kh",
        "uvhy62avo1wqms1rrtefth84xhnv1a59aez6r4xq0pla74036o3vznihxexwydnfjojmk6ipl6",
        "0t0dlfopg27cqv1xp4qfgwdlivvgqz204hkh5ianbb4abgk0yjolcwhhitrcksha5s6otmps0hd",
        "vrbhcwrmn5xbq8f518ntvmaeg89n7nh1uxebfsmd7smoog3k2w12zv0px32pf4b78er5f3pgy7b9",
        "x5bmnefocbtxm8avt22ekuy5hcdyxh86is5fnns9ycfm7o25x9frwv9kfv2ohyd3txlc8zlg5rjjx",
        "ttfrgnfvvj552vjymrqqd1yjlyff7vkffprnvu3co4vuah8y0s56tziih3yowm64ja810gb1sgk0um",
        "a66t43i9vrr3cmg5qf52akuk8bxl4rm3i86rm7h5brjou9k2egrzy3h19hh8kqr2queyvrwb673qikj",
        "mfuwhbvd88n21obpmwx273mmeqiz98qfmb04z0ute54kc1d9bbdyfbx2sc4em6t4pfektm05qs7bgc9z",
        "x8wbm0kjpyua8wpgsejgxc06geitm1c0bxihvcwnxnif63dj7cygzk7led0z49ol6zf2xwcmf99n4osip",
        "fvba43myr0ozab882crozdz0zx4lfl2h7xe2phfqte97g58fake2fzi87mpftz9qdmt45gm79xl43k1hji",
        "wnr0pz08rm3j65b7pl116l59pxy6prnydf9xod1qdi3hp3lod2vuzy1v7gt2g72sejaomn5u53daxjrr9xk",
        "bwo7nfqda6w56voyvg1nr7vkq61zi7gy0aggn6pic3gup7uy18zzsc7y5yz3ptvp5cd53i95dj521k4n6n7t",
        "mromebynw459uydhhgcgrate6hnst5srng9knfjc02vtg1vywok3rdbw935pf1qwghnh0nibyb60l9elkmajg",
        "59dcjawsd4kjjcceco3hphizua88l0qtrfd000iam3rnb4tmy6kzf5bhkc9ud1hsg3dd53tlsxarcl0n59081h",
        "odgdgfkwcpz0zjcwsz9is5h4nhebzht7fqa1b4g8e2snb6bn5hu3ixyd2pk1ey5g3eab0m3aoknfi9ctkpxz07j",
        "0ljqm7r10ns2pjo8x69oi0zuqss9y7301yd6rmex8djwrbqmvh2mbwscgj9pmrgul5ao0tvpefpe5a9cac5xbdwb",
        "b449ak3ihp8tdrbteffru5vboeh1z63c55at3qz70p13d2fim50q8i06zjyb53i4gqzunx6rsl07jxjd9g77me1ww",
        "oqzf6c40snvrjz4v0f4h8p0ozjfy1y4xihxwaz16vbxf3qsa805xodw8z5xq3hb7dag8fnxtlsc62150kk253i3buj",
        "2eicp9a5aq2uycq55y7rsixlg3pfk7gyin65fghf03kks18dixbckxmbv5xnhyrir7qm8maz4rk2bi3zs9chidlhehf",
        "7k1wyjs6fxss4e0ywqfurgop6f7y7e97f3mr5hnb0hlhqkqbqvi1e1z3qfyxc3te75r67fc4h9li06rl9zadg3v9zmz6",
        "k3e403zdtia8i0gpodm00yaujr1w474bh3985o3csbfjp3dll4t98i5lesloo6rqjec2aycb3ttx1t6lg0cl9hrjkgheb",
        "2fv8zdl1ljmpjbvaan0nt99tra48yjmc5pv91n1c5l8qp5pv77zwsx75ouay7bmgy2tjc1aazyu5zj7oimesavv9n2h7ky",
        "ghxs7uejpzpbxjsdmc2w9fabrg4j4pwwbn0wjxux2luk1k0ciror4gcvww18e610u2wpczuwrcphy2xr1129vweqhhgitge",
        "vk7wfi9hhi0j9n2grs8rxgq68kw54dbdviuxnvtwgz77h0qkbzqw7pgm7zgn21cxlxnyzigeyz2rzrj3awloq86tqe60e070",
        "d1aot9216s547uk1rg651iscb1bjpgth5j4f6arx1902npcykk8niz3ffpbed47idgzvt4u59fyi5e0e2afpjb5gjk4rysn8j",
        "2jef2xl4o9yub0z6jnxu8gm87g9iv9zdtu9yolvxtensjrtgplnmnuhz43nsxztk8s936k6eruckkiwc5hnch4qdzft093986x",
        "oo70ed77jci4bgodhnyf37axrx4f8gf8qs94f4l9xi9h0jkdl2ozoi2p7q7qu1945l21dzj6rhvqearzrmblfo3ljjldj0m9fue"
    };

    private ulong[] POLYMUR_REFERENCE_VALUES =
    {
        0x1a6ef9f9d6c576fbUL, 0xd16d059771c65e13UL, 0x5ee4e0c09f562f87UL, 0x535b5311db007b0bUL,
        0xd17124f14bd16b5dUL, 0xe84c87105c5b5cadUL, 0xb16ce684b89df9c0UL, 0x656525cace200667UL,
        0x92b460794885d16dUL, 0xe6cc0fd9725b46b9UL, 0xc875ade1929bc93dUL, 0x68a2686ced37268aUL,
        0x1d1809fd7e7e14efUL, 0x699b8f31fc40c137UL, 0xd10dca2605654d2dUL, 0xd6bc75cb729f18d7UL,
        0xfe0c617e7cb1bffeUL, 0xf5f14c731c1b9a22UL, 0x7a0382228d248631UL, 0x6c3a5f49d8a48bc0UL,
        0x3606ebe637bb4ebcUL, 0xeb4854d75431ad1dUL, 0xfa8ff1a34793ebb0UL, 0x7e46ad8e2338cc38UL,
        0xf8ff088ada3154b4UL, 0x706669bf0925914fUL, 0x70fc5fbcd3485aceUL, 0x96fd279baed2f2abUL,
        0x6403a64c68d7bf68UL, 0x3f8f532e1df472e5UL, 0xbfc49c083515596fUL, 0xd678a4b338fbf03bUL,
        0x127142a2f38b70a1UL, 0x8a1a56fbb85b71f6UL, 0x961d22b14e6f1932UL, 0xa166b0326c942c30UL,
        0x0f3d837dddb86ae2UL, 0x0f8164504b4ea8b1UL, 0xe4f6475d5a739af4UL, 0xbf535ad625c0d51fUL,
        0x47f10a5a13be50adUL, 0x3dc5ce9c148969b3UL, 0x8dc071fb4df8e144UL, 0x9d0a83586cbed3b8UL,
        0xc4379e22f2809b99UL, 0x42010c7dd7657650UL, 0xcc31a6fbcdab8be8UL, 0x7bad06c38400138aUL,
        0x0178b41584eb483dUL, 0x78afc38d52514efcUL, 0x65a57c4e59288dc7UL, 0x86e7cc3e273e4e47UL,
        0xeb99661fb41a6bd2UL, 0xea0979aa6cd70febUL, 0xa64a347c0b8e007bUL, 0x3692969270fe8fa4UL,
        0x17640c6052e26555UL, 0xdf9e0fd276291357UL, 0x64cca6ebf4580720UL, 0xf82b33f6399c3f49UL,
        0xbe3ccb7526561379UL, 0x8c796fce8509c043UL, 0x9849fded8c92ce51UL, 0xa0e744d838dbc4efUL,
        0x8e4602d33a961a65UL, 0xda381d6727886a7eUL, 0xa503a344fc066833UL, 0xbf8ff5bc36d5dc7bUL,
        0x795ae9ed95bca7e9UL, 0x19c80807dc900762UL, 0xea7d27083e6ca641UL, 0xeba7e4a637fe4fb5UL,
        0x34ac9bde50ce9087UL, 0xe290dd0393f2586aUL, 0xbd7074e9843d9dcaUL, 0x66c17140a05887e6UL,
        0x4ad7b3e525e37f94UL, 0xde0d009c18880dd6UL, 0x1516bbb1caca46d3UL, 0xe9c907ec28f89499UL,
        0xd677b655085e1e14UL, 0xac5f949b08f29553UL, 0xd353b06cb49b5503UL, 0x9c25eb30ffa8cc78UL,
        0x6cf18c91658e0285UL, 0x99264d2b2cc86a77UL, 0x8b438cd1bb8fb65dUL, 0xdfd56cf20b217732UL,
        0x71f4e35bf761bacfUL, 0x87d7c01f2b11659cUL, 0x95de608c3ad2653cUL, 0x51b50e6996b8de93UL,
        0xd21e837b2121e8c9UL, 0x73d07c7cb3fa0ba7UL, 0x8113fab03cab6df3UL, 0x57cdddea972cc490UL,
        0xc3df94778f1eec30UL, 0x7509771e4127701eUL, 0x28240c74c56f8f7cUL, 0x194fa4f68aab8e27UL
    };

    [Fact]
    public void TestVectors()
    {
        for (int i = 0; i < POLYMUR_TEST_STRINGS.Length; i++)
        {
            string s = POLYMUR_TEST_STRINGS[i];
            ulong h = Polymur2Hash64.ComputeHash(Encoding.ASCII.GetBytes(s), 0xabcdef0123456789UL);
            ulong expected = POLYMUR_REFERENCE_VALUES[i];

            if (expected != h)
                Assert.Fail($"Test at index {i} failed with string {s}\nExpected {expected:X}, got {h:X}");
        }
    }
}