---
title: Agenda des communautés tech toulousaines
layout: default
---

<script type="text/javascript">
  function copyToClipboard(name) {
    var elt = document.getElementById(name);
    if(!elt) return;
    var url = elt.getAttribute('data-url');
    /*
    elt.select();
    elt.setSelectionRange(0, 99999); // for mobile devices??
    */
    navigator.clipboard.writeText(url);
    
    var tooltip = bootstrap.Tooltip.getOrCreateInstance(elt, { title: 'URL copiée !', trigger: 'manual'});
    tooltip.show();
    setTimeout(function() { tooltip.hide()}, 3000);
  }
</script>

<!-- lead -->
<section class="py-1 text-center container">
  <div class="row">
    <div class="col-lg-6 col-md-8 mx-auto">
      <img src="logo.png" style="width: 100%" alt="Logo Toulouse Tech Hub" />
      <p class="lead text-muted">
          Retrouvez toutes les informations sur les communautés tech toulousaines sur une seule page.
      </p>
    </div>
  </div>
</section>

<!-- agenda -->
<section class="container" id="agenda">

  <!-- title -->
  <div class="row">
    <div class="mx-auto text-center">
      <h1 class="fw-light">Agenda</h1>
      <p class="lead text-muted">
          Tous les prochains évènements des communautés tech toulousaines.
      </p>
    </div>
  </div>

  <!-- ics / atom -->
  <div class="row justify-content-center">
    <div class="my-2 col-lg-4 col-md-8">
      <div class="input-group">
        <span class="input-group-text"><i class="bi bi-calendar2-week"></i>&nbsp;iCal</span>
        <input id="icsInput" type="text" value="{{ site.site }}{{ site.baseurl }}/events.ics" data-url="{{ site.site }}{{ site.baseurl }}/events.ics" readonly class="form-control" style="background-color:#fff" onfocus="this.select()">
        <button class="btn btn-outline-secondary" type="button" id="button-addon-ics" title="Copier l'URL dans le presse-papier" onclick="copyToClipboard('icsInput')"><i class="bi bi-clipboard-check"></i></button>
      </div>
    </div>
    <div class="my-2 col-lg-4 col-md-8">
      <div class="input-group">
        <span class="input-group-text"><i class="bi bi-rss"></i>&nbsp;Rss/Atom</span>
        <input id="atomInput" type="text" value="{{ site.site }}{{ site.baseurl }}/events.atom.xml" data-url="{{ site.site }}{{ site.baseurl }}/events.atom.xml" readonly class="form-control" style="background-color:#fff" onfocus="this.select()">
        <button class="btn btn-outline-secondary" type="button" id="button-addon-atom" title="Copier l'URL dans le presse-papier" onclick="copyToClipboard('atomInput')"><i class="bi bi-clipboard-check"></i></button>
      </div>
    </div>
  </div>

  <!-- json -->
  <div class="row justify-content-center">
    <div class="my-2 col-lg-4 col-md-8">
      <div class="input-group">
        <span class="input-group-text"><i class="bi bi-braces"></i>&nbsp;Json</span>
        <input id="jsonInput" type="text" value="{{ site.site }}{{ site.baseurl }}/events.json" data-url="{{ site.site }}{{ site.baseurl }}/events.json" readonly class="form-control" style="background-color:#fff" onfocus="this.select()">
        <button class="btn btn-outline-secondary" type="button" id="button-addon-json" title="Copier l'URL dans le presse-papier" onclick="copyToClipboard('jsonInput')"><i class="bi bi-clipboard-check"></i></button>
      </div>
    </div>
  </div>

  <!-- calendars -->
  <div class="row my-4 gy-3">
    <div class="col-lg-6 col-xxl-4">
      {% assign current_month = site.time | date: "%m" %}
      {% assign current_year = site.time | date: "%Y" %}
      {% assign current_day = site.time | date: "%d" %}
      {% include calendar.html month=current_month year=current_year today=current_day %}
    </div>
    <div class="col-lg-6 col-xxl-4">
      {% assign next_month = site.time | date: "%m" | plus: 1 %}
      {% assign next_year = site.time | date: "%Y" %}
      {% if next_month > 12 %}
        {% assign next_month = 1 %}
        {% assign next_year = current_year | plus: 1 %}
      {% endif %}
      {% assign next_day = 0 %}
      {% include calendar.html month=next_month year=next_year today=next_day %}
    </div>
    <div class="col-lg-6 col-xxl-4 calendar-third">
      {% assign third_month = site.time | date: "%m" | plus: 2 %}
      {% assign third_year = site.time | date: "%Y" %}
      {% if third_month > 12 %}
        {% assign third_month = third_month | minus: 12 %}
        {% assign third_year = current_year | plus: 1 %}
      {% endif %}
      {% assign third_day = 0 %}
      {% include calendar.html month=third_month year=third_year today=third_day %}
    </div>
  </div>

  <!-- grid -->
  <div class="row row-cols-1 row-cols-md-2 row-cols-xxl-3 g-3 agenda my-2">

{%- assign now_time = site.time | date: "%s" | plus: 0 -%}
{%- for event in site.events -%}
  {%- assign event_time = event.dateIso | date:"%s" | plus: 0 -%}
  {%- if event_time < now_time -%}
    {%- continue -%}
  {%- endif -%}

  {%- assign event_date = event.dateIso | date: "%Y-%m-%d" -%}
  <div class="col" data-event-date="{{ event_date }}">
    <div class="card shadow h-100">
      <div class="card-header">🧑‍💻 {{ event.community }}</div>
      <a href="{{ event.link }}">
        <img width="100%" style="aspect-ratio: 16/9" src="{{ site.baseurl }}/{{ event.localImg }}" />
        <div class="card-body">
          <h5 class="card-title">{{ event.title }}</h5>
          <p class="card-text">
          </p>
          <div class="d-flex justify-content-between align-items-center text-right">
            <small class="text-muted place">
            {%- if event.place != null and event.place != "" -%}
            🏠 {{ event.place }}<br>📍 {{ event.placeAddr }}
            {%- else −%}
            🌍 en ligne
            {%- endif -%}
            </small>
            <small class="text-muted text-end time">{{ event.dateFr }}📅<br>{{ event.timeFr }}⌚</small>
          </div>
        </div>
      </a>
    </div>
  </div>

{%- endfor -%}

  </div>
</section>

<!-- conférences -->
<section class="container my-2" id="conferences">

  <!-- title -->
  <div class="row">
    <div class="mx-auto text-center">
      <h1 class="fw-light">Conférences</h1>
      <p class="lead text-muted">
        Les grandes conférences tech annuelles sur Toulouse !
      </p>
    </div>
  </div>

  {% include conferences.html %}

</section>

<!-- communautés -->
<section class="container my-2" id="communautes">

  <!-- title -->
  <div class="row ">
    <div class="mx-auto text-center">
      <h1 class="fw-light">Communautés</h1>
      <p class="lead text-muted">
        La liste des communautés tech toulousaines référencées.
      </p>
    </div>
  </div>

  <!-- list -->
  <div class="row row-cols-1 row-cols-md-2 row-cols-xxl-3 g-4 my-3">
  {% assign groups_by_name = site.groups | sort: "name" %}
  {% for group in groups_by_name %}
  {%- capture gradient_class -%}
    {%- if group.id == "gdg" -%}community-gdg
    {%- elsif group.id == "mtg" -%}community-mtg
    {%- elsif group.id == "agile" -%}community-agile
    {%- elsif group.id == "tgd" or group.name == "Toulouse Game Dev" -%}community-tgd
    {%- elsif group.id == "ruby" -%}community-ruby
    {%- elsif group.id == "jug" -%}community-jug
    {%- elsif group.id == "python" -%}community-python
    {%- elsif group.id == "js" -%}community-js
    {%- else -%}community-default
    {%- endif -%}
  {%- endcapture -%}
  <div class="col">
    {%- assign group_img_url = "" -%}
    {%- if group.img -%}
      {%- if group.img contains '://' -%}
        {%- assign group_img_url = group.img -%}
      {%- else -%}
        {%- assign group_img_url = site.baseurl | append: group.img -%}
      {%- endif -%}
    {%- endif -%}
    <div class="card community-card{% if group_img_url == "" %} {{ gradient_class | strip }}{% endif %}{% if group_img_url != "" %} community-card-with-img{% endif %}"{% if group_img_url != "" %} style="background-image: url('{{ group_img_url }}');"{% endif %}>
      <div class="community-card-body">
        <h5 class="community-card-title">{{ group.name }}</h5>
        {% if group.content %}
        <p class="community-card-desc">{{ group.content | strip_html | truncatewords: 15 }}</p>
        {% elsif group.description %}
        <p class="community-card-desc">{{ group.description | strip_html | truncatewords: 15 }}</p>
        {% endif %}
        <div class="community-card-links">
          <a href="{{ group.url }}" title="Site Web" target="_blank"><i class="bi bi-globe"></i></a>
          {% if group.social %}
            {% for social in group.social %}
              {% if social.name == "x" %}
              <a href="{{ social.url }}" title="X / Twitter" target="_blank"><i class="bi bi-twitter-x"></i></a>
              {% elsif social.name == "linkedin" %}
              <a href="{{ social.url }}" title="LinkedIn" target="_blank"><i class="bi bi-linkedin"></i></a>
              {% elsif social.name == "github" %}
              <a href="{{ social.url }}" title="GitHub" target="_blank"><i class="bi bi-github"></i></a>
              {% elsif social.name == "mastodon" %}
              <a href="{{ social.url }}" title="Mastodon" target="_blank"><i class="bi bi-mastodon"></i></a>
              {% endif %}
            {% endfor %}
          {% endif %}
        </div>
      </div>
    </div>
  </div>
  {% endfor %}
  </div>

</section>

<footer class="text-muted py-5">
  <div class="container">
    <p class="float-end mb-1">
      <a href="#">⬆️Retour en haut</a>
    </p>
    <p>
      ❓Votre communauté ou votre évènement n'est pas présent et vous souhaitez qu'il soit ajouté ?<br>
      👉Venez le signaler sur le <a href="https://github.com/GDGToulouse/toulouse-tech-hub">projet github.com</a>.<br>
      ⚖️<a href="mentions.html">Mentions légales</a>
    </p>
    <!-- <p class="mb-1">Album example is © Bootstrap, but please download and customize it for yourself!</p> -->
    <!-- <p class="mb-0">New to Bootstrap? <a href="/">Visit the homepage</a> or read our <a href="/docs/5.0/getting-started/introduction/">getting started guide</a>.</p> -->
  </div>
</footer>
