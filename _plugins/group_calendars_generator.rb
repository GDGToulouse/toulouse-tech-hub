# frozen_string_literal: true

module GroupCalendars
  class GroupCalendarPage < Jekyll::PageWithoutAFile
    def initialize(site, group)
      slug = group.data["slug"] || group.basename_without_ext

      @site = site
      @base = site.source
      @dir = "groups/#{slug}"
      @name = "calendar.ics"

      process(@name)

      @data = {
        "layout" => "group-calendar",
        "group_slug" => slug,
        "title" => "#{group.data['name'] || slug} calendar",
        "sitemap" => false
      }
    end
  end

  class GroupCalendarsGenerator < Jekyll::Generator
    safe true
    priority :low

    def generate(site)
      groups = site.collections.fetch("groups", nil)
      return unless groups

      groups.docs.each do |group|
        site.pages << GroupCalendarPage.new(site, group)
      end
    end
  end
end
