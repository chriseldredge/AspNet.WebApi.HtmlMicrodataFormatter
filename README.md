This library enhances your Asp.net WebApi project by adding a MediaTypeFormatter
that formats arbitrary objects as well-formed html5 documents with embedded
microdata attributes for entities, properties and values.

The library also includes a controller that generates documentation,
forms and links based on your routes, parameters, controllers and actions.
Enabling this controller to respond to requests, e.g. to `~/api`,
greatly improves how easily clients can discover available resources and
actions and the parameters they accept.

Using this library enables clients that follow HATEOAS (Hypermedia as the Engine of Application State)
conventions that prevent tight coupling, magic URLs and brittle formatting.

## Available on NuGet Gallery

To install the [AspNet.WebApi.HtmlMicrodataFormatter package](http://nuget.org/packages/AspNet.WebApi.HtmlMicrodataFormatter),
run the following command in the [Package Manager Console](http://docs.nuget.org/docs/start-here/using-the-package-manager-console)

    PM> Install-Package AspNet.WebApi.HtmlMicrodataFormatter

## Example

Given this code:

```c#
public class TodoController : ApiController
{
    public Todo Get(int id)
    {
        return new Todo
            {
                Name = "Finish this app",
                Description = "It'll take 6 to 8 weeks.",
                Due = DateTime.Now.AddDays(7*6)
            };
    }

    public class Todo
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime Due { get; set; }
    }
}
```

The following markup is rendered:

```html
<html>
    <body>
        <dl itemtype="http://schema.org/Thing" itemscope="itemscope">
            <dt>Name</dt>
            <dd><span itemprop="name">Finish this app</span></dd>
            <dt>Description</dt>
            <dd><span itemprop="description">It'll take 6 to 8 weeks.</span></dd>
            <dt>Due</dt>
            <dd><time datetime="2013-09-04T12:59:31Z" itemprop="due">Wed, 04 Sep 2013 12:59:31 GMT</time></dd>
        </dl>
    </body>
</html>
```

When DocumentationController is configured (see below), the following markup is rendered:

```html
<section class="api-group" id="Todo">
    <header><h1>Todo</h1></header>
    <section class="api">
        <h2>Get</h2>
        <p>GET /api/todo/get</p>
        <form action="/api/todo/get" method="GET" data-templated="false">
            <label>
                id
                <input name="id" type="text" value="" data-required="true" data-calling-convention="query-string">
            </label>
            <input type="submit">
        </form>
    </section>
</section>
```

## Configuring HtmlMicrodataFormatter

This sample configures a project to respond to client requests that ask for
text/html or application/xhtml+xml (or text/xml or application/xml) with
html5 microdata documents.

```c#
public static class WebApiConfig
{
    public static void Register(HttpConfiguration config)
    {
        // optional: remove XmlFormatter to serve text/xml and application/xml requests with HtmlMicrodataFormatter
        config.Formatters.Remove(config.Formatters.XmlFormatter);

        config.Formatters.Add(CreateHtmlMicrodataFormatter());
    }

    private static MediaTypeFormatter CreateHtmlMicrodataFormatter()
    {
        var formatter = new HtmlMicrodataFormatter();

        // optional: insert css and javascript
        formatter.AddHeadContent(new XElement("title", "My WebApi Project")
        formatter.AddHeadContent(new XElement("link",
                      new XAttribute("rel", "stylesheet"),
                      new XAttribute("href", "//netdna.bootstrapcdn.com/twitter-bootstrap/2.3.2/css/bootstrap-combined.min.css")));

        // optional: custom serializers can control how a specific Type is rendered as html:
        formatter.RegisterSerializer(new ToStringSerializer(typeof (Version)));

        return formatter;
    }
}
```

## Configuring DocumentationController

This sample configures the documentation controller which generates
html5 forms and links to registered routes including inputs for
parameters.

```c#
public static class WebApiConfig
{
    public static void Register(HttpConfiguration config)
        var documentation = new HtmlDocumentation();
        documentation.Load();

        config.Services.Replace(typeof(IDocumentationProvider), new WebApiHtmlDocumentationProvider(documentation));
    }

    public static void MapApiRoutes(HttpConfiguration config)
    {
        config.Routes.MapHttpRoute("RouteNames.Documentation",
                            "api",
                            new { controller = "Documentation" });
    }
}
```

The `HtmlDocumentation.Load` method will look for assembly documentation xml files in the
private bin path of the application and load them to show controller and action documentation
alongside form elements.

## Links

The driving force behind REST and HATEOAS is using links (and forms) as the only means of
transitioning from one state to another. Thus when a response comes from submitting a form
or following a link, the response should in turn contain its own links to related
resources and functionality.

AspNet.WebApi.HtmlMicrodataFormatter.Link can be placed on your response objects to create links
that include rel attributes.

## Uri Templates

The forms generated by DocumentationController may have URIs that contain template expressions,
e.g. /api/records/{id}. If the action has one or more template expressions, the form will
have the attribute `data-templated="true"`. Clients should use an [RFC6570](http://tools.ietf.org/html/rfc6570)
template processor on these URIs before submitting them.

This will prevent testing such actions from a browser. This issue can be worked around
by injecting javascript into the documentation (using the AddHeadContent method) where
the javascript hooks into form.submit events and adjusts the action as needed.

## Customizing Serialization

Serialization is delegated to IHtmlMicrodataSerializer instances which
are registered by type. The first IHtmlMicrodataSerializer that handles
a type or ancestor type is chosen.

The following serializers are automatically registered:

 * UriSerializer: renders System.Uri as `<a>` elements
 * LinkSerializer: renders `AspNet.WebApi.HtmlMicrodataFormatter.Link` as `<a>` with `rel=` attribute
 * DateTimeSerializer: renders System.DateTime and System.DateTimeOffset as `<time>` elements
 * TimeSpanSerializer: renders System.TimeSpan as `<time>` elements using ISO8601 format
 * ToStringSerializer: renders objects as `<span>` elements by invoking ToString() on them
 * ApiDocumentationSerializer: renders SimpleApiDocumentation (used with DocumentationController)
 * ApiDescriptionSerializer: renders SimpleApiDescription as `<form>` (or `<a>` for parameterless actions)

If none of the registered serializers supports a given type, DefaultSerializer will be used.

By default, ToStringSerializer does not handle any types. You can add types as follows:

```c#
var htmlMicrodataFormatter = new HtmlMicrodataFormatter();
htmlMicrodataFormatter.ToStringSerializer.AddSupportedTypes(typeof(Version));
```

You can register your own serializers by calling `htmlMicrodataFormatter.RegisterSerializer()`.

You can choose to implement IHtmlMicrodataSerializer directly or subclass from either
DefaultSerializer or EntitySerializer.

## Customizing itemprop

You can control how property names are converted into itemprop attribute values
by setting PropNameProvider on HtmlMicrodataFormatter. The default is to
convert them to camel case.

## TODO

- Helper to assign prev, self, next to list of links
- Mechanism to control order properties are rendered in
- Ability to hide ApiDescription
- Ability to provide top-level documentation
- Add more custom input types (checkbox, textarea, select, file, etc)
- Transform `<param/>` and `<returns/>` elements
- Dynamic c# client
- Microdata compatible schema from type documentation
